using FileTypeChecker.Abstracts;

namespace DroughtPrediction.Services.DataLoading;
public class FileTypeChecker
{
    public class Excel : FileType, IFileType
    {
        public const string TypeName = "Excel";
        public const string TypeExtension = "xlsx";
        private static readonly byte[] MagicBytes = [0x50, 0x4B, 0x03, 0x04, 0x14, 0x00, 0x06, 0x00, 0x08, 0x00, 0x00, 0x00, 0x21, 0x00];

        public Excel() : base(TypeName, TypeExtension, MagicBytes) { }
    }

    public class NetCDF : FileType, IFileType
    {
        public const string TypeName = "NetCDF";
        public const string TypeExtension = "nc";
        private static readonly byte[] MagicBytes = [0x89, 0x48, 0x44, 0x46];

        public NetCDF() : base(TypeName, TypeExtension, MagicBytes) { }
    }
}
