using ContainerExpressions.Containers;

namespace Wass.Core.Services
{
    public sealed class Format : Option
    {
        public static readonly Format Video = new(nameof(Video));
        public static readonly Format Image = new(nameof(Image));
        public static readonly Format Audio = new(nameof(Audio));
        public static readonly Format Document = new(nameof(Document));
        public static readonly Format Executable = new(nameof(Executable));

        private Format(string value) : base(value) { }
    }

    public static class Category
    {
        public static Either<Format, NotFound> GetFormat(string extension)
        {
            if (extension.StartsWith('.')) extension = extension[1..];

            if (_video.Contains(extension)) return Format.Video;
            if (_image.Contains(extension)) return Format.Image;
            if (_audio.Contains(extension)) return Format.Audio;
            if (_document.Contains(extension)) return Format.Document;
            if (_executable.Contains(extension)) return Format.Executable;

            return new NotFound();
        }

        public static bool IsCompressed(string extension)
        {
            if (extension.StartsWith('.')) extension = extension[1..];
            return _compressed.Contains(extension);
        }

        private static readonly HashSet<string> _video = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "3g2", // 3GPP2 Multimedia File
            "3gp", // 3GPP Multimedia File
            "amv", // Anime Music Video file (Specific to some portable media players)
            "asf", // Advanced Systems Format (Can contain Windows Media Video/Audio)
            "avi", // Audio Video Interleave file
            "drc", // Dirac video file (Less common as a standalone extension, often in other containers)
            "divx", // DivX Video file (A specific container used by DivX)
            "dvr-ms", // Microsoft Digital Video Recording file
            "f4a", // FLV/MP4 based audio (Included as it's Flash-related, though primarily audio)
            "f4b", // FLV/MP4 based audiobook (Included as it's Flash-related)
            "f4p", // FLV/MP4 based protected media (Included as it's Flash-related)
            "f4v", // Flash Video file (Based on the MP4 container)
            "flv", // Flash Video file
            "gxf", // General eXchange Format (Professional/broadcast standard)
            "m2v", // MPEG-2 Video Elementary Stream (Often used in DVD authoring)
            "m2ts", // MPEG-2 Transport Stream (Used in Blu-ray discs and AVCHD camcorders)
            "m4v", // MP4 Video file (Often used by Apple/iTunes, may have DRM)
            "mkv", // Matroska Video file
            "mk3d", // Matroska 3D Video file
            "mov", // Apple QuickTime Movie
            "mp4", // MPEG-4 Video file
            "mpe", // MPEG Movie file (Less common alternative for .mpeg/.mpg)
            "mpeg", // MPEG Movie file
            "mpg", // MPEG Movie file
            "mpv", // MPEG-1/2 Video Elementary Stream (Often used in DVD authoring)
            "mts", // AVCHD Video file (MPEG Transport Stream variant, from cameras)
            "mxf", // Material Exchange Format (Professional video/broadcast standard)
            "nsv", // Nullsoft Streaming Video file
            "ogv", // Ogg Video file
            "qt", // Apple QuickTime Movie (Older extension for .mov)
            "rm", // RealMedia file
            "rmvb", // RealMedia Variable Bitrate file
            "swf", // Shockwave Flash Movie (Can contain video, but often interactive)
            "ts", // MPEG Transport Stream (Used in broadcast and streaming)
            "vob", // DVD Video Object (Files found on DVD video discs)
            "webm", // WebM video file (Designed for the web)
            "wmv", // Windows Media Video file
            "wtv", // Windows Recorded TV Show (Used by Windows Media Center)
            "yuv" // YUV Raw Video file (Uncompressed video data)
        };

        private static readonly HashSet<string> _image = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "jpg", // JPEG (Joint Photographic Experts Group) - Very common for photographs, uses lossy compression.
            "jpeg", // JPEG (Joint Photographic Experts Group) - Very common for photographs, uses lossy compression.
            "jpe", // JPEG (Joint Photographic Experts Group) - Very common for photographs, uses lossy compression.
            "jif", // JPEG (Joint Photographic Experts Group) - Very common for photographs, uses lossy compression.
            "jfif", // JPEG (Joint Photographic Experts Group) - Very common for photographs, uses lossy compression.
            "jfi", // JPEG (Joint Photographic Experts Group) - Very common for photographs, uses lossy compression.
            "png", // PNG (Portable Network Graphics) - Supports lossless compression and transparency, widely used for web graphics and images requiring sharp lines or transparency.
            "gif", // GIF (Graphics Interchange Format) - Supports lossless compression, transparency, and animation. Limited color palette (256 colors).
            "bmp", // BMP (Bitmap) - Older format, often uncompressed (or uses simple RLE compression), typically large file sizes.
            "dib", // BMP (Bitmap) - Older format, often uncompressed (or uses simple RLE compression), typically large file sizes.
            "tif", // TIFF (Tagged Image File Format) - Supports lossless and lossy compression, widely used in printing and publishing. Can store multiple layers/pages.
            "tiff", // TIFF (Tagged Image File Format) - Supports lossless and lossy compression, widely used in printing and publishing. Can store multiple layers/pages.
            "webp", // WebP - Modern format developed by Google, designed for the web. Supports lossy and lossless compression, transparency, and animation. Offers good compression ratios.
            "heif", // HEIF (High Efficiency Image File Format), HEIC (High Efficiency Image Container) - Modern format used by Apple and others. Offers better compression than JPEG, supports single images or sequences, transparency, and depth maps. Can contain HEVC encoded images.
            "heic", // HEIF (High Efficiency Image File Format), HEIC (High Efficiency Image Container) - Modern format used by Apple and others. Offers better compression than JPEG, supports single images or sequences, transparency, and depth maps. Can contain HEVC encoded images.
            "avif", // AVIF (AV1 Image File Format) - Newer format based on the AV1 video codec. Offers high compression efficiency and features like HDR and wide color gamut.
            "apng", // APNG (Animated Portable Network Graphics) - Extension to PNG that supports animation, similar to GIF but with better color support and transparency.
            "jp2", // JPEG 2000 - Successor to JPEG, uses wavelet compression, supports lossy and lossless. Not as widely adopted as original JPEG.
            "j2k", // JPEG 2000 - Successor to JPEG, uses wavelet compression, supports lossy and lossless. Not as widely adopted as original JPEG.
            "jpx", // JPEG 2000 - Successor to JPEG, uses wavelet compression, supports lossy and lossless. Not as widely adopted as original JPEG.
            "jpm", // JPEG 2000 - Successor to JPEG, uses wavelet compression, supports lossy and lossless. Not as widely adopted as original JPEG.
            "tga", // TGA (Truevision TGA, or TARGA) - Older format, often used in video and animation industries. Supports various color depths and optional alpha channel.
            "dds", // DDS (DirectDraw Surface) - Primarily used for storing textures and cubemaps in real-time graphics applications (games).
            "mng", // MNG (Multiple-image Network Graphics) - Complex format related to PNG, supports advanced animation features. Not widely adopted.
            "svg", // SVG (Scalable Vector Graphics) - XML-based vector format for 2D graphics. Scales without loss of quality. .svgz is a gzip-compressed version.
            "svgz", // SVG (Scalable Vector Graphics) - XML-based vector format for 2D graphics. Scales without loss of quality. .svgz is a gzip-compressed version.
            "ai", // AI (Adobe Illustrator Artwork) - Proprietary vector format used by Adobe Illustrator. Can contain both vector and embedded/linked raster images.
            "eps", // EPS (Encapsulated PostScript) - Older vector format, can contain vector graphics, raster images, or both. Used in publishing workflows.
            "cdr", // CDR (CorelDRAW) - Proprietary vector format used by CorelDRAW.
            "dng", // DNG (Digital Negative) - Adobe's attempt at a universal RAW format.
            "crw", // Canon RAW
            "cr2", // Canon RAW
            "cr3", // Canon RAW
            "nef", // Nikon RAW
            "nrw", // Nikon RAW
            "arw", // Sony RAW
            "srf", // Sony RAW
            "sr2", // Sony RAW
            "orf", // Olympus RAW
            "rw2", // Panasonic RAW
            "pef", // Pentax RAW
            "raf", // Fujifilm RAW
            "3fr", // Hasselblad RAW
            "psd", // PSD (Photoshop Document) - Proprietary format used by Adobe Photoshop. Can contain layers, masks, transparency, text, vector data, and raster images.
            "xcf", // XCF (eXperimental Computing Facility) - Native format for the GIMP image editor. Supports layers, transparency, etc.
            "ico", // ICO - Format used for computer icons, typically on Windows. Can contain multiple sizes and color depths within one file.
            "icns" // ICNS - Format used for computer icons on macOS. Can contain multiple sizes and resolutions.
        };

        private static readonly HashSet<string> _audio = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "aac", // Advanced Audio Coding (lossy compression)
            "aa", // Audible Audiobook (proprietary, often with DRM)
            "aax", // Audible Enhanced Audiobook (proprietary, often with DRM)
            "ac3", // Dolby Digital (lossy, often multichannel audio)
            "aif", // Audio Interchange File Format (uncompressed, common on Apple systems)
            "aiff", // Audio Interchange File Format (uncompressed, common on Apple systems)
            "alac", // Apple Lossless Audio Codec (lossless compression, often found in .m4a containers)
            "amr", // Adaptive Multi-Rate (voice codec, common on mobile phones)
            "ape", // Monkey's Audio (lossless compression)
            "au", // Au file format (older format, common on Unix systems)
            "caf", // Core Audio Format (container format developed by Apple)
            "dff", // Direct Stream Digital File (high-resolution audio format from Sony/Philips)
            "dsf", // DSD Stream File (high-resolution audio format, similar to DFF)
            "dss", // Digital Speech Standard (proprietary format for dictation)
            "dts", // Digital Theater System (lossy, often multichannel audio)
            "flac", // Free Lossless Audio Codec (lossless compression)
            "gsm", // GSM 06.10 (codec used in mobile telephony)
            "it", // Impulse Tracker (tracker format containing samples and sequence data)
            "m4a", // MPEG-4 Audio (container, often holds AAC or ALAC; widely used)
            "m4b", // MPEG-4 Audiobook (similar to M4A, supports bookmarks)
            "m4p", // MPEG-4 Protected Audio (iTunes format with DRM)
            "mid", // Musical Instrument Digital Interface (contains musical note and control data, not sampled audio)
            "midi", // Musical Instrument Digital Interface (contains musical note and control data, not sampled audio)
            "mod", // Module file (Amiga/PC tracker format)
            "mp2", // MPEG-1 Audio Layer II (older lossy format)
            "mp3", // MPEG-1 Audio Layer III (the most common lossy format)
            "mpc", // Musepack / MP+ (lossy compression)
            "oga", // Ogg Vorbis (container, often holds Vorbis, but can hold Opus, Speex, FLAC etc.)
            "ogg", // Ogg Vorbis (container, often holds Vorbis, but can hold Opus, Speex, FLAC etc.)
            "opus", // Opus Interactive Audio Codec (highly efficient codec, often in .ogg or .opus containers)
            "qcp", // Qualcomm PureVoice (voice codec)
            "shn", // Shorten (older lossless format)
            "snd", // Sound file (generic extension, can represent various formats like AU or AIFF)
            "spx", // Speex (voice codec)
            "tta", // True Audio (lossless compression)
            "voc", // Creative Labs Voice (older format used by Sound Blaster cards)
            "vox", // Dialogic ADPCM (older voice format)
            "wav", // Waveform Audio File Format (container, often holds uncompressed PCM audio, but can hold others)
            "wave", // Waveform Audio File Format (container, often holds uncompressed PCM audio, but can hold others)
            "wma", // Windows Media Audio (Microsoft format, can be lossy or lossless)
            "wv", // WavPack (lossless compression, supports hybrid lossy/correction mode)
            "xm" // FastTracker II (tracker format containing samples and sequence data)
        };

        private static readonly HashSet<string> _document = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "doc", // Microsoft Word Document - Binary and XML-based
            "docx", // Microsoft Word Document - Binary and XML-based
            "odt", // OpenDocument Text - LibreOffice, OpenOffice.org
            "rtf", // Rich Text Format
            "txt", // Plain Text
            "wpd", // WordPerfect Document
            "wp", // WordPerfect Document
            "wps", // WordPerfect Document
            "pages", // Apple Pages Document
            "abw", // AbiWord Document
            "sam", // AmiPro Document
            "lwp", // Lotus Word Pro Document
            "sxw", // OpenOffice.org 1.x Writer Document
            "xls", // Microsoft Excel Spreadsheet - Binary and XML-based
            "xlsx", // Microsoft Excel Spreadsheet - Binary and XML-based
            "ods", // OpenDocument Spreadsheet - LibreOffice, OpenOffice.org
            "csv", // Comma Separated Values - Plain text table
            "tsv", // Tab Separated Values - Plain text table
            "numbers", // Apple Numbers Spreadsheet
            "sxc", // OpenOffice.org 1.x Calc Spreadsheet
            "ppt", // Microsoft PowerPoint Presentation - Binary and XML-based
            "pptx", // Microsoft PowerPoint Presentation - Binary and XML-based
            "odp", // OpenDocument Presentation - LibreOffice, OpenOffice.org
            "key", // Apple Keynote Presentation
            "keynote", // Apple Keynote Presentation
            "sxi", // OpenOffice.org 1.x Impress Presentation
            "pdf", // Adobe Portable Document Format
            "fdf", // Forms Data Format - PDF related
            "xfdf", // XML Forms Data Format - PDF related
            "xps", // XML Paper Specification
            "djvu", // DjVu - Often used for scanned technical documents/books
            "ps", // PostScript
            "epub", // Electronic Publication
            "mobi", // Mobipocket E-book
            "prc", // Mobipocket E-book
            "azw", // Amazon Kindle Formats
            "azw3", // Amazon Kindle Formats
            "kpf", // Amazon Kindle Formats
            "iba", // iBooks Author Document
            "kf8", // Kindle Format 8
            "html", // HyperText Markup Language
            "htm", // HyperText Markup Language
            "xhtml", // Extensible HyperText Markup Language
            "md", // Markdown
            "markdown", // Markdown
            "rst", // reStructuredText
            "tex", // LaTeX Source Document
            "xml", // Extensible Markup Language - used for structured documents like DocBook, DITA, etc.
            "dbk", // DocBook XML
            "dita", // DITA XML
            "adoc", // AsciiDoc
            "asciidoc", // AsciiDoc
            "chm", // Compiled HTML Help - Often used for documentation/e-books
            "dot", // Microsoft Word Templates
            "dotx", // Microsoft Word Templates
            "ott", // OpenDocument Text Template
            "xlt", // Microsoft Excel Templates
            "xltx", // Microsoft Excel Templates
            "ots", // OpenDocument Spreadsheet Template
            "pot", // Microsoft PowerPoint Templates
            "potx", // Microsoft PowerPoint Templates
            "otp", // OpenDocument Presentation Template
            "vsd", // Microsoft Visio Drawing - Often treated as a document/diagram
            "vsdx" // Microsoft Visio Drawing - Often treated as a document/diagram
        };

        private static readonly HashSet<string> _executable = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "sh", // Shell script (Bourne shell, Bash, etc. - Unix/Linux/macOS)
            "pl", // Perl script (Unix/Linux/macOS, Windows via Perl interpreter)
            "py", // Python script (Cross-platform via Python interpreter)
            "rb", // Ruby script (Cross-platform via Ruby interpreter)
            "php", // PHP script (Often web-related, but can be run via CLI)
            "js", // JavaScript (Used for Node.js on server/CLI, also client-side in browsers, but Node.js files are directly executable)
            "jar", // Java Archive (Executable if it contains a manifest specifying a main class)
            "bin", // Generic Binary file (Can be executable, especially on embedded systems or older platforms, sometimes raw executables on Linux/Unix)
            "exe", // Primary Windows Executable
            "com", // Older DOS/Windows Executable (simple format)
            "bat", // Batch script
            "cmd", // Command script (newer Batch)
            "ps1", // PowerShell script
            "vbs", // VBScript file
            "jse", // JScript file (.jse is encoded)
            "wsf", // Windows Script File
            "scr", // Screen Saver (often just a renamed .exe)
            "cpl", // Control Panel Item (a type of DLL executed by rundll32.exe)
            "dll", // Dynamic Link Library (contains executable code, run by other programs/utilities like rundll32.exe)
            "lnk", // Shortcut file (While not the executable itself, it's an executable instruction to run another file, often an executable)
            "hta", // HTML Application
            "msi", // Microsoft Installer Package
            "msp", // Microsoft Installer Patch
            "appref-ms", // ClickOnce Application Reference
            "sys", // System file (often drivers or core components, can contain executable code)
            "app", // macOS Application Bundle (This is a directory containing the executable and resources, but the .app extension identifies the executable package)
            "command" // Executable script on macOS, often opened via Terminal
        };

        private static readonly HashSet<string> _compressed = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "zip", // ZIP Archive: Most common archive format (uses Deflate primarily).
            "rar", // RAR Archive: Roshal Archive (Proprietary compression, often high).
            "7z", // 7-Zip Archive: Often uses LZMA/LZMA2 for high compression.
            "tar.gz", // TAR Archive (Gzipped): Tape Archive compressed with Gzip.
            "tgz", // TAR Archive (Gzipped): Shorthand for .tar.gz.
            "tar.bz2", // TAR Archive (Bzipped2): Tape Archive compressed with Bzip2.
            "tbz", // TAR Archive (Bzipped2): Shorthand for .tar.bz2.
            "tbz2", // TAR Archive (Bzipped2): Shorthand for .tar.bz2.
            "tar.xz", // TAR Archive (XZ): Tape Archive compressed with XZ (LZMA/LZMA2).
            "txz", // TAR Archive (XZ): Shorthand for .tar.xz.
            "tar.Z", // TAR Archive (Compressed): Tape Archive compressed with Unix compress utility.
            "gz", // Gzip Compressed File: Typically compresses a single file (often a .tar).
            "bz2", // Bzip2 Compressed File: Typically compresses a single file (often better than Gzip, slower).
            "xz", // XZ Compressed File: Typically compresses a single file (uses LZMA/LZMA2, often best).
            "Z", // Compress Compressed File: Older Unix utility compression.
            "jar", // Java Archive: Based on ZIP format (for Java code/resources).
            "war", // Web Application Archive: Java web application archive (based on ZIP).
            "ear", // Enterprise Application Archive: Java enterprise archive (based on ZIP).
            "apk", // Android Package: Android application package (based on ZIP).
            "xpi", // Cross-Platform Installer: Firefox/Thunderbird extension (based on ZIP).
            "crx", // Chrome Extension: Chrome browser extension (uses ZIP format).
            "nupkg", // NuGet Package: Package for .NET libraries (based on ZIP).
            "whl", // Python Wheel: Python built distribution format (based on ZIP).
            "egg", // Python Egg: Older Python distribution format (based on ZIP).
            "epub", // Electronic Publication: E-book format (based on ZIP).
            "lha", // LHA Archive: Lempel-Ziv-Huffman archive (older format).
            "lzh", // LZH Archive: Lempel-Ziv-Huffman archive (older format).
            "arj", // ARJ Archive: Older DOS-era archive format.
            "cab", // Cabinet File: Microsoft archive format (for software distribution).
            "deb", // Debian Package: Linux package (contains compressed archives like tar.gz/xz).
            "rpm", // RPM Package: Linux package (contains compressed cpio archive).
            "sit", // StuffIt Archive: macOS archive format (older).
            "sitx", // StuffIt X Archive: Newer macOS archive format.
            "dmg", // Apple Disk Image: macOS disk image (can be compressed).
            "pkg", // macOS Installer Package: Often contains compressed archives.
            "mp3", // MP3 Audio: MPEG Audio Layer III (lossy compression).
            "aac", // AAC Audio: Advanced Audio Coding (lossy compression).
            "ogg", // Ogg Vorbis Audio: Open source audio format (lossy compression).
            "wma", // Windows Media Audio: Microsoft audio format (usually lossy, can be lossless).
            "opus", // Opus Audio: Modern efficient audio codec (lossy compression). 
            "m4a", // MPEG-4 Audio: Often contains AAC or ALAC (can be lossy or lossless).
            "flac", // FLAC Audio: Free Lossless Audio Codec (lossless compression).
            "alac", // Apple Lossless Audio Codec: Apple's lossless audio (often in .m4a).
            "ape", // Monkey's Audio: Lossless audio compression format.
            "wv", // WavPack Audio: Lossless (and lossy hybrid) audio compression.
            "tta", // True Audio: Lossless audio compression format.
            "mp4", // MPEG-4 Video: Common video container (uses H.264, HEVC etc. compressed codecs).
            "mkv", // Matroska Video: Flexible video container (uses various compressed codecs).
            "mov", // QuickTime Video: Apple video container (uses various compressed codecs like H.264, ProRes).
            "avi", // Audio Video Interleave: Older video container (uses various compressed codecs).
            "wmv", // Windows Media Video: Microsoft video container/codec (compressed).
            "webm", // WebM Video: Open video format for web (VP8/VP9/AV1 compressed video).
            "flv", // Flash Video: Older web video format (uses compressed codecs).
            "mpg", // MPEG Video: MPEG-1/MPEG-2 video (compressed).
            "mpeg", // MPEG Video: MPEG-1/MPEG-2 video (compressed).
            "m2ts", // MPEG-2 Transport Stream: Used for Blu-ray/AVCHD (compressed).
            "ts", // Transport Stream: MPEG transport stream (compressed).
            "3gp", // 3GPP Multimedia File: Mobile video format (compressed).
            "3g2", // 3GPP2 Multimedia File: Mobile video format (compressed).
            "ogv", // Ogg Video: Video container (often uses Theora compressed codec).
            "vob", // DVD Video Object: Contains MPEG-2 compressed video/audio.
            "jpg", // JPEG Image: Joint Photographic Experts Group (lossy compression, for photos).
            "jpeg", // JPEG Image: Joint Photographic Experts Group (lossy compression, for photos). 
            "heic", // HEIC Image: High Efficiency Image Format (lossy compression, uses HEVC).
            "heif", // HEIF Image: High Efficiency Image File Format (lossy compression).
            "png", // PNG Image: Portable Network Graphics (lossless compression, uses Deflate).
            "gif", // GIF Image: Graphics Interchange Format (lossless LZW compression, limited colors).
            "webp", // WebP Image: Google image format (supports both lossy and lossless compression).
            "jp2", // JPEG 2000 Image: Image format (supports lossless and lossy compression).
            "jpx", // JPEG 2000 Image: Image format (supports lossless and lossy compression).
            "j2k", // JPEG 2000 Image: Image format (supports lossless and lossy compression).
            "tiff", // TIFF Image: Tagged Image File Format (can use lossless compression like LZW/Deflate, or be uncompressed).
            "tif", // TIFF Image: Tagged Image File Format (can use lossless compression like LZW/Deflate, or be uncompressed).
            "psd", // Adobe Photoshop Document: Uses lossless RLE or ZIP compression internally.
            "bmp", // Bitmap Image: Usually uncompressed, but can use lossless RLE compression.
            "docx", // Microsoft Word Document: Open XML format (ZIP archive containing XML).
            "xlsx", // Microsoft Excel Spreadsheet: Open XML format (ZIP archive containing XML).
            "pptx", // Microsoft PowerPoint Presentation: Open XML format (ZIP archive containing XML)
            "odt", // OpenDocument Text: Open Document Format for text (ZIP archive).
            "ods", // OpenDocument Spreadsheet: Open Document Format for spreadsheets (ZIP archive).
            "odp", // OpenDocument Presentation: Open Document Format for presentations (ZIP archive).
            "odg", // OpenDocument Graphics: Open Document Format for graphics (ZIP archive).
            "pdf", // Portable Document Format: Can contain compressed text, fonts, and images (Flate, LZW, JPEG etc.).
            "xps", // XML Paper Specification: Microsoft's PDF alternative (ZIP-based).
            "oxps", // Open XML Paper Specification: Open XPS format (ZIP-based).
            "key", // Apple Keynote Presentation: Package format often involving compression
            "pages", // Apple Pages Document: Package format often involving compression.
            "numbers", // Apple Numbers Spreadsheet: Package format often involving compression.
            "swf", // Shockwave Flash: Adobe Flash file (uses Zlib or LZMA compression).
            "svgz", // Compressed SVG: Scalable Vector Graphics file compressed with Gzip.
            "dwf", // Autodesk Design Web Format: Compressed format for CAD drawings.
            "dwfx", // Autodesk Design Web Format XPS: XPS-based compressed format for CAD (ZIP-based).
            "chm", // Compiled HTML Help File: Microsoft help format (uses LZX compression).
            "vmdk", // VMware Virtual Disk: Some variants support compression.
            "qcow2" // QEMU Copy-On-Write Disk Image: Virtual disk format supporting compression (zlib/zstd).
        };
    }
}
