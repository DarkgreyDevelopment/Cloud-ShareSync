using Microsoft.VisualStudio.TestTools.UnitTesting;
using Cloud_ShareSync.Core.CloudProvider.BackBlaze.Threading;
using System.Collections.Generic;
using System;

namespace Core {
    [TestClass]
    public class TestThreadQueueDetails {
        public const long B = 1;
        public const long KiB = B * 1024;
        public const long MiB = KiB * 1024;
        public const long GiB = MiB * 1024;
        public const long TiB = GiB * 1024;
        public const int MinSize = (int)(MiB * 5);
        public const int RecSize = (int)(MiB * 100);
        public const int ThreadCount = 15;

        [TestMethod]
        public void TestMethod1( ) {

        }

        [DataTestMethod]
        [DynamicData( nameof( GetTestData ), DynamicDataSourceType.Method )]
        public void MyTest( IEnumerable<string> myStrings ) {
            // ...  
        }

        public static IEnumerable<object[]> GetTestData( ) {
            yield return new object[] { new List<Tuple<long,ThreadQueueDetails?,Exception?>>( ) {
                new(
                    MinSize,
                    new ThreadQueueDetails(){
                        _threadCount = ThreadCount;
                        _partSize    = MinSize;
                        _finalSize   = MinSize;
                        _totalParts  = 1;
                        _fileSize    = (long)MinSize
                    },
                    null
                ),
                RecSize,
                new(
                    RecSize,
                    new ThreadQueueDetails(){
                        _threadCount = ThreadCount;
                        _partSize    = 6990506;
                        _finalSize   = MinSize;
                        _totalParts  = ThreadCount;
                        _fileSize    = (long)MinSize
                    },
                    null
                ),
                RecSize * ThreadCount,
                (RecSize * ThreadCount) + B,
                B,
                KiB,
                MiB,
                GiB,
                TiB,
            } };
        }
    }
}
