namespace SevenZip
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics;
#if NET472 || NETSTANDARD2_0
    using System.Security.Permissions;
#endif
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Text;

#if UNMANAGED
    /// <summary>
    /// 7-zip library low-level wrapper.
    /// </summary>
    internal static class SevenZipLibraryManager
    {
        /// <summary>
        /// Synchronization root for all locking.
        /// </summary>
        private static readonly object SyncRoot = new object();

        /// <summary>
        /// 7-zip library features.
        /// </summary>
        private static LibraryFeature? _features;

        private static Dictionary<object, Dictionary<InArchiveFormat, IInArchive>> _inArchives;
        private static Dictionary<object, Dictionary<OutArchiveFormat, IOutArchive>> _outArchives;
        private static int _totalUsers;

        private static void InitUserInFormat(object user, InArchiveFormat format)
        {
            if (!_inArchives.ContainsKey(user))
            {
                _inArchives.Add(user, new Dictionary<InArchiveFormat, IInArchive>());
            }

            if (!_inArchives[user].ContainsKey(format))
            {
                _inArchives[user].Add(format, null);
                _totalUsers++;
            }
        }

        private static void InitUserOutFormat(object user, OutArchiveFormat format)
        {
            if (!_outArchives.ContainsKey(user))
            {
                _outArchives.Add(user, new Dictionary<OutArchiveFormat, IOutArchive>());
            }

            if (!_outArchives[user].ContainsKey(format))
            {
                _outArchives[user].Add(format, null);
                _totalUsers++;
            }
        }

        private static void Init()
        {
            _inArchives = new Dictionary<object, Dictionary<InArchiveFormat, IInArchive>>();
            _outArchives = new Dictionary<object, Dictionary<OutArchiveFormat, IOutArchive>>();
        }

        /// <summary>
        /// Loads the 7-zip library if necessary and adds user to the reference list
        /// </summary>
        /// <param name="user">Caller of the function</param>
        /// <param name="format">Archive format</param>
        public static void LoadLibrary(object user, Enum format)
        {
            lock (SyncRoot)
            {
                if (_inArchives == null || _outArchives == null)
                {
                    Init();
                }

                if (format is InArchiveFormat archiveFormat)
                {
                    InitUserInFormat(user, archiveFormat);
                    return;
                }

                if (format is OutArchiveFormat outArchiveFormat)
                {
                    InitUserOutFormat(user, outArchiveFormat);
                    return;
                }

                throw new ArgumentException($"Enum {format} is not a valid archive format attribute!");
            }
        }

        static readonly string Namespace = Assembly.GetExecutingAssembly().GetManifestResourceNames()[0].Split('.')[0];

        private static string GetResourceString(string str)
        {
            return Namespace + ".arch." + str;
        }

        private static bool ExtractionBenchmark(string archiveFileName, Stream outStream, ref LibraryFeature? features, LibraryFeature testedFeature)
        {
            var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(GetResourceString(archiveFileName));
            
            try
            {
                using (var extractor = new SevenZipExtractor(stream))
                {
                    extractor.ExtractFile(0, outStream);
                }
            }
            catch (Exception)
            {
                return false;
            }

            features |= testedFeature;
            return true;
        }

        public static LibraryFeature CurrentLibraryFeatures
        {
            get
            {
                lock (SyncRoot)
                {
                    if (_features.HasValue)
                    {
                        return _features.Value;
                    }

                    _features = LibraryFeature.None;

                    #region Benchmark

                    #region Extraction features

                    using (var outStream = new MemoryStream())
                    {
                        ExtractionBenchmark("Test.lzma.7z", outStream, ref _features, LibraryFeature.Extract7z);
                        ExtractionBenchmark("Test.lzma2.7z", outStream, ref _features, LibraryFeature.Extract7zLZMA2);

                        var i = 0;

                        if (ExtractionBenchmark("Test.bzip2.7z", outStream, ref _features, _features.Value))
                        {
                            i++;
                        }

                        if (ExtractionBenchmark("Test.ppmd.7z", outStream, ref _features, _features.Value))
                        {
                            i++;
                            if (i == 2 && (_features & LibraryFeature.Extract7z) != 0 &&
                                (_features & LibraryFeature.Extract7zLZMA2) != 0)
                            {
                                _features |= LibraryFeature.Extract7zAll;
                            }
                        }

                        ExtractionBenchmark("Test.rar", outStream, ref _features, LibraryFeature.ExtractRar);
                        ExtractionBenchmark("Test.tar", outStream, ref _features, LibraryFeature.ExtractTar);
                        ExtractionBenchmark("Test.txt.bz2", outStream, ref _features, LibraryFeature.ExtractBzip2);
                        ExtractionBenchmark("Test.txt.gz", outStream, ref _features, LibraryFeature.ExtractGzip);
                        ExtractionBenchmark("Test.txt.xz", outStream, ref _features, LibraryFeature.ExtractXz);
                        ExtractionBenchmark("Test.zip", outStream, ref _features, LibraryFeature.ExtractZip);
                    }

                    #endregion

                    #endregion

                    return _features.Value;
                }
            }
        }

        /// <summary>
        /// Removes user from reference list and frees the 7-zip library if it becomes empty
        /// </summary>
        /// <param name="user">Caller of the function</param>
        /// <param name="format">Archive format</param>
        public static void FreeLibrary(object user, Enum format)
        {
#if NET472 || NETSTANDARD2_0
            var sp = new SecurityPermission(SecurityPermissionFlag.UnmanagedCode);
            sp.Demand();
#endif
            lock (SyncRoot)
			{
                if (format is InArchiveFormat archiveFormat)
                {
                    if (_inArchives != null && _inArchives.ContainsKey(user) &&
                        _inArchives[user].ContainsKey(archiveFormat) &&
                        _inArchives[user][archiveFormat] != null)
                    {
                        try
                        {
                            Marshal.ReleaseComObject(_inArchives[user][archiveFormat]);
                        }
                        catch (InvalidComObjectException) { }

                        _inArchives[user].Remove(archiveFormat);
                        _totalUsers--;

                        if (_inArchives[user].Count == 0)
                        {
                            _inArchives.Remove(user);
                        }
                    }
                }

                if (format is OutArchiveFormat outArchiveFormat)
                {
                    if (_outArchives != null && _outArchives.ContainsKey(user) &&
                        _outArchives[user].ContainsKey(outArchiveFormat) &&
                        _outArchives[user][outArchiveFormat] != null)
                    {
                        try
                        {
                            Marshal.ReleaseComObject(_outArchives[user][outArchiveFormat]);
                        }
                        catch (InvalidComObjectException) { }

                        _outArchives[user].Remove(outArchiveFormat);
                        _totalUsers--;

                        if (_outArchives[user].Count == 0)
                        {
                            _outArchives.Remove(user);
                        }
                    }
                }

                if ((_inArchives == null || _inArchives.Count == 0) && (_outArchives == null || _outArchives.Count == 0))
                {
                    _inArchives = null;
                    _outArchives = null;
                }
            }
        }

        /// <summary>
        /// Gets IInArchive interface to extract 7-zip archives.
        /// </summary>
        /// <param name="format">Archive format.</param>
        /// <param name="user">Archive format user.</param>
        public static IInArchive InArchive(InArchiveFormat format, object user)
        {
            lock (SyncRoot)
            {
                if (!_inArchives.ContainsKey(user) || _inArchives[user][format] == null)
                {
#if NET472 || NETSTANDARD2_0
                    var sp = new SecurityPermission(SecurityPermissionFlag.UnmanagedCode);
                    sp.Demand();
#endif

                    //var createObject = (NativeMethods.CreateObjectDelegate)
                    //    Marshal.GetDelegateForFunctionPointer(
                    //        NativeMethods.GetProcAddress(_modulePtr, "CreateObject"),
                    //        typeof(NativeMethods.CreateObjectDelegate));

                    //if (createObject == null)
                    //{
                    //    throw new SevenZipLibraryException();
                    //}

                    object result;
                    var interfaceId = typeof(IInArchive).GUID;
                    var classId = Formats.InFormatGuids[format];

                    try
                    {
                        NativeMethods.CreateObject(in classId, in interfaceId, out result);
                    }
                    catch (Exception)
                    {
                        throw new SevenZipLibraryException("Your 7-zip library does not support this archive type.");
                    }

                    InitUserInFormat(user, format);									
                    _inArchives[user][format] = result as IInArchive;
                }

                return _inArchives[user][format];
            }
        }

        /// <summary>
        /// Gets IOutArchive interface to pack 7-zip archives.
        /// </summary>
        /// <param name="format">Archive format.</param>  
        /// <param name="user">Archive format user.</param>
        public static IOutArchive OutArchive(OutArchiveFormat format, object user)
        {
            lock (SyncRoot)
            {
                if (_outArchives[user][format] == null)
                {
#if NET472 || NETSTANDARD2_0
                    var sp = new SecurityPermission(SecurityPermissionFlag.UnmanagedCode);
                    sp.Demand();
#endif

                    //var createObject = (NativeMethods.CreateObjectDelegate)
                    //    Marshal.GetDelegateForFunctionPointer(
                    //        NativeMethods.GetProcAddress(_modulePtr, "CreateObject"),
                    //        typeof(NativeMethods.CreateObjectDelegate));
                    var interfaceId = typeof(IOutArchive).GUID;
                    

                    try
                    {
                        var classId = Formats.OutFormatGuids[format];
                        NativeMethods.CreateObject(in classId, in interfaceId, out var result);
                        
                        InitUserOutFormat(user, format);
                        _outArchives[user][format] = result as IOutArchive;
                    }
                    catch (Exception)
                    {
                        throw new SevenZipLibraryException("Your 7-zip library does not support this archive type.");
                    }
                }

                return _outArchives[user][format];
            }
        }
    }
#endif
}
