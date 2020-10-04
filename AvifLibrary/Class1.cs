using System;

namespace AvifLibrary
{

    public unsafe partial class AVIF
    {
        public const int AVIF_VERSION_MAJOR = 0;
        public const int AVIF_VERSION_MINOR = 8;
        public const int AVIF_VERSION_PATCH = 1;
        public const int AVIF_VERSION = (AVIF_VERSION_MAJOR * 10000) + (AVIF_VERSION_MINOR * 100) + AVIF_VERSION_PATCH;
        public const int AVIF_QUANTIZER_LOSSLESS = 0;
        public const int AVIF_QUANTIZER_BEST_QUALITY = 0;
        public const int AVIF_QUANTIZER_WORST_QUALITY = 63;
        public const int AVIF_PLANE_COUNT_YUV = 3;
        public const int AVIF_SPEED_DEFAULT = -1;
        public const int AVIF_SPEED_SLOWEST = 0;
        public const int AVIF_SPEED_FASTEST = 10;
        public enum AvifPlanes
        {
            YUV = (1 << 0),
            A = (1 << 1),
            ALL = 0xff
        }
        public enum AvifChannelIndex
        {
            // rgbPlanes
            AVIF_CHAN_R = 0,
            AVIF_CHAN_G = 1,
            AVIF_CHAN_B = 2,

            // yuvPlanes
            AVIF_CHAN_Y = 0,
            AVIF_CHAN_U = 1,
            AVIF_CHAN_V = 2
        }
        public partial string avifVersion();
        public partial void avifCodecVersions(ref char[] outBuffer);
        public partial void* avifAlloc(uint size);
        public partial void avifFree(void* p);
        public enum avifResult
        {
            OK = 0,
            UNKNOWN_ERROR,
            INVALID_FTYP,
            NO_CONTENT,
            NO_YUV_FORMAT_SELECTED,
            REFORMAT_FAILED,
            UNSUPPORTED_DEPTH,
            ENCODE_COLOR_FAILED,
            ENCODE_ALPHA_FAILED,
            BMFF_PARSE_FAILED,
            NO_AV1_ITEMS_FOUND,
            DECODE_COLOR_FAILED,
            DECODE_ALPHA_FAILED,
            COLOR_ALPHA_SIZE_MISMATCH,
            ISPE_SIZE_MISMATCH,
            NO_CODEC_AVAILABLE,
            NO_IMAGES_REMAINING,
            INVALID_EXIF_PAYLOAD,
            INVALID_IMAGE_GRID,
            INVALID_CODEC_SPECIFIC_OPTION,
            TRUNCATED_DATA,
            NO_IO,
            IO_ERROR,
            WAITING_ON_IO
        }
        public partial string avifResultToString(avifResult result);
        public struct avifROData
        {
            public readonly ushort* data;
            public uint size;
        }
        public struct avifRWData
        {
            public ushort* data;
            public uint size;
        }
        public partial void avifRWDataRealloc(avifRWData* raw, uint newSize);
        public partial void avifRWDataSet(avifRWData* raw, ushort* data, uint len);
        public partial void avifRWDataFree(avifRWData* raw);
        public enum avifPixelFormat
        {
            NONE = 0,

            YUV444,
            YUV422,
            YUV420,
            YUV400
        }
        public partial string avifPixelFormatToString(avifPixelFormat format);
        public struct avifPixelFormatInfo
        {
            public bool monochrome;
            public int chromaShiftX;
            public int chromaShiftY;
        }
        public partial void avifGetPixelFormatInfo(avifPixelFormat format, ref avifPixelFormatInfo info);
        public enum avifChromaSamplePosition
        {
            UNKNOWN = 0,
            VERTICAL = 1,
            COLOCATED = 2
        }
        public enum avifRange
        {
            LIMITED = 0,
            FULL = 1
        }
        public enum avifColorPrimaries
        {
            UNKNOWN = 0,

            BT709 = 1,
            IEC61966_2_4 = 1,
            UNSPECIFIED = 2,
            BT470M = 4,
            BT470BG = 5,
            BT601 = 6,
            SMPTE240 = 7,
            GENERIC_FILM = 8,
            BT2020 = 9,
            XYZ = 10,
            SMPTE431 = 11,
            SMPTE432 = 12, // DCI P3
            EBU3213 = 22
        }
        public partial void avifColorPrimariesGetValues(avifColorPrimaries acp, float[] outPrimaries);
        public partial avifColorPrimaries avifColorPrimariesFind(float[] inPrimaries, char** outName);
        public enum avifTransferCharacteristics
        {
            // This is actually reserved, but libavif uses it as a sentinel value.
            UNKNOWN = 0,

            BT709 = 1,
            UNSPECIFIED = 2,
            BT470M = 4,  // 2.2 gamma
            BT470BG = 5, // 2.8 gamma
            BT601 = 6,
            SMPTE240 = 7,
            LINEAR = 8,
            LOG100 = 9,
            LOG100_SQRT10 = 10,
            IEC61966 = 11,
            BT1361 = 12,
            SRGB = 13,
            BT2020_10BIT = 14,
            BT2020_12BIT = 15,
            SMPTE2084 = 16, // PQ
            SMPTE428 = 17,
            HLG = 18
        }
        public enum avifMatrixCoefficients
        {
            IDENTITY = 0,
            BT709 = 1,
            UNSPECIFIED = 2,
            FCC = 4,
            BT470BG = 5,
            BT601 = 6,
            SMPTE240 = 7,
            YCGCO = 8,
            BT2020_NCL = 9,
            BT2020_CL = 10,
            SMPTE2085 = 11,
            CHROMA_DERIVED_NCL = 12,
            CHROMA_DERIVED_CL = 13,
            ICTCP = 14
        }
        public enum avifTransformationFlags
        {
            NONE = 0,

            PASP = (1 << 0),
            CLAP = (1 << 1),
            IROT = (1 << 2),
            IMIR = (1 << 3)
        }
        public struct avifPixelAspectRatioBox
        {
            // 'pasp' from ISO/IEC 14496-12:2015 12.1.4.3

            // define the relative width and height of a pixel
            public UInt32 hSpacing;
            public UInt32 vSpacing;
        }
        public struct avifCleanApertureBox
        {
            // 'clap' from ISO/IEC 14496-12:2015 12.1.4.3

            // a fractional number which defines the exact clean aperture width, in counted pixels, of the video image
            public UInt32 widthN;
            public UInt32 widthD;

            // a fractional number which defines the exact clean aperture height, in counted pixels, of the video image
            public UInt32 heightN;
            public UInt32 heightD;

            // a fractional number which defines the horizontal offset of clean aperture centre minus (width‐1)/2. Typically 0.
            public UInt32 horizOffN;
            public UInt32 horizOffD;

            // a fractional number which defines the vertical offset of clean aperture centre minus (height‐1)/2. Typically 0.
            public UInt32 vertOffN;
            public UInt32 vertOffD;
        }
        public struct avifImageRotation
        {
            // 'irot' from ISO/IEC 23008-12:2017 6.5.10

            // angle * 90 specifies the angle (in anti-clockwise direction) in units of degrees.
            public ushort angle; // legal values: [0-3]
        }
        public struct avifImageMirror
        {
            // 'imir' from ISO/IEC 23008-12:2017 6.5.12

            // axis specifies a vertical (axis = 0) or horizontal (axis = 1) axis for the mirroring operation.
            public ushort axis; // legal values: [0, 1]
        }
        public struct avifImage
        {
            // Image information
            public UInt32 width;
            public UInt32 height;
            public UInt32 depth; // all planes must share this depth; if depth>8, all planes are uint16_t internally

            public avifPixelFormat yuvFormat;
            public avifRange yuvRange;
            public avifChromaSamplePosition yuvChromaSamplePosition;
            public ushort*[] yuvPlanes;
            public UInt32[] yuvRowBytes;
            public bool imageOwnsYUVPlanes;

            public avifRange alphaRange;
            public ushort* alphaPlane;
            public UInt32 alphaRowBytes;
            public bool imageOwnsAlphaPlane;

            // ICC Profile
            public avifRWData icc;

            // CICP information:
            // These are stored in the AV1 payload and used to signal YUV conversion. Additionally, if an
            // ICC profile is not specified, these will be stored in the AVIF container's `colr` box with
            // a type of `nclx`. If your system supports ICC profiles, be sure to check for the existence
            // of one (avifImage.icc) before relying on the values listed here!
            public avifColorPrimaries colorPrimaries;
            public avifTransferCharacteristics transferCharacteristics;
            public avifMatrixCoefficients matrixCoefficients;

            // Transformations - These metadata values are encoded/decoded when transformFlags are set
            // appropriately, but do not impact/adjust the actual pixel buffers used (images won't be
            // pre-cropped or mirrored upon decode). Basic explanations from the standards are offered in
            // comments above, but for detailed explanations, please refer to the HEIF standard (ISO/IEC
            // 23008-12:2017) and the BMFF standard (ISO/IEC 14496-12:2015).
            //
            // To encode any of these boxes, set the values in the associated box, then enable the flag in
            // transformFlags. On decode, only honor the values in boxes with the associated transform flag set.
            public UInt32 transformFlags;
            public avifPixelAspectRatioBox pasp;
            public avifCleanApertureBox clap;
            public avifImageRotation irot;
            public avifImageMirror imir;

            // Metadata - set with avifImageSetMetadata*() before write, check .size>0 for existence after read
            public avifRWData exif;
            public avifRWData xmp;
        }
    }
    public unsafe partial class AVIF
    {
        public readonly string AVIF_VERSION_STRING = AVIF_VERSION_MAJOR + "." + AVIF_VERSION_MINOR + "." + AVIF_VERSION_PATCH;
        public partial string avifVersion()
        {
            return AVIF_VERSION_STRING;
        }
        public partial string avifPixelFormatToString(avifPixelFormat format)
        {
            switch (format) {
                case avifPixelFormat.YUV444:
                    return "YUV444";
                case avifPixelFormat.YUV420:
                    return "YUV420";
                case avifPixelFormat.YUV422:
                    return "YUV422";
                case avifPixelFormat.YUV400:
                    return "YUV400";
                case avifPixelFormat.NONE:
                default:
                    break;
            }
            return "Unknown";
        }
        public partial void avifGetPixelFormatInfo(avifPixelFormat format, ref avifPixelFormatInfo info)
        {
            info = new avifPixelFormatInfo();

            switch (format)
            {
                case avifPixelFormat.YUV444:
                    info.chromaShiftX = 0;
                    info.chromaShiftY = 0;
                    break;

                case avifPixelFormat.YUV422:
                    info.chromaShiftX = 1;
                    info.chromaShiftY = 0;
                    break;

                case avifPixelFormat.YUV420:
                    info.chromaShiftX = 1;
                    info.chromaShiftY = 1;
                    break;

                case avifPixelFormat.YUV400:
                    info.chromaShiftX = 1;
                    info.chromaShiftY = 1;
                    info.monochrome = true;
                    break;

                case avifPixelFormat.NONE:
                default:
                    break;
            }
        }
        public partial string avifResultToString(avifResult result)
        {
            // clang-format off
            switch (result)
            {
                case avifResult.OK: return "OK";
                case avifResult.INVALID_FTYP: return "Invalid ftyp";
                case avifResult.NO_CONTENT: return "No content";
                case avifResult.NO_YUV_FORMAT_SELECTED: return "No YUV format selected";
                case avifResult.REFORMAT_FAILED: return "Reformat failed";
                case avifResult.UNSUPPORTED_DEPTH: return "Unsupported depth";
                case avifResult.ENCODE_COLOR_FAILED: return "Encoding of color planes failed";
                case avifResult.ENCODE_ALPHA_FAILED: return "Encoding of alpha plane failed";
                case avifResult.BMFF_PARSE_FAILED: return "BMFF parsing failed";
                case avifResult.NO_AV1_ITEMS_FOUND: return "No AV1 items found";
                case avifResult.DECODE_COLOR_FAILED: return "Decoding of color planes failed";
                case avifResult.DECODE_ALPHA_FAILED: return "Decoding of alpha plane failed";
                case avifResult.COLOR_ALPHA_SIZE_MISMATCH: return "Color and alpha planes size mismatch";
                case avifResult.ISPE_SIZE_MISMATCH: return "Plane sizes don't match ispe values";
                case avifResult.NO_CODEC_AVAILABLE: return "No codec available";
                case avifResult.NO_IMAGES_REMAINING: return "No images remaining";
                case avifResult.INVALID_EXIF_PAYLOAD: return "Invalid Exif payload";
                case avifResult.INVALID_IMAGE_GRID: return "Invalid image grid";
                case avifResult.INVALID_CODEC_SPECIFIC_OPTION: return "Invalid codec-specific option";
                case avifResult.TRUNCATED_DATA: return "Truncated data";
                case avifResult.NO_IO: return "No IO";
                case avifResult.IO_ERROR: return "IO Error";
                case avifResult.WAITING_ON_IO: return "Waiting on IO";
                case avifResult.UNKNOWN_ERROR:
                default:
                    break;
            }
            // clang-format on
            return "Unknown Error";
        }
        // This function assumes nothing in this struct needs to be freed; use avifImageClear() externally
        public static void avifImageSetDefaults(ref avifImage image)
        {
            image = new avifImage();
            image.yuvRange = avifRange.FULL;
            image.alphaRange = avifRange.FULL;
            image.colorPrimaries = avifColorPrimaries.UNSPECIFIED;
            image.transferCharacteristics = avifTransferCharacteristics.UNSPECIFIED;
            image.matrixCoefficients = avifMatrixCoefficients.UNSPECIFIED;
        }
        public avifImage avifImageCreate(int width, int height, int depth, avifPixelFormat yuvFormat)
        {
            avifImage image = new avifImage();
            avifImageSetDefaults(ref image);
            image.width = (uint)width;
            image.height = (uint)height;
            image.depth = (uint)depth;
            image.yuvFormat = yuvFormat;
            return image;
        }
        public avifImage avifImageCreateEmpty()
        {
            return avifImageCreate(0, 0, 0, avifPixelFormat.NONE);
        }
        public void avifImageCopy(avifImage dstImage, avifImage srcImage, UInt32 planes)
        {
            avifImageFreePlanes(dstImage, AvifPlanes.ALL);

            dstImage.width = srcImage.width;
            dstImage.height = srcImage.height;
            dstImage.depth = srcImage.depth;
            dstImage.yuvFormat = srcImage.yuvFormat;
            dstImage.yuvRange = srcImage.yuvRange;
            dstImage.yuvChromaSamplePosition = srcImage.yuvChromaSamplePosition;
            dstImage.alphaRange = srcImage.alphaRange;

            dstImage.colorPrimaries = srcImage.colorPrimaries;
            dstImage.transferCharacteristics = srcImage.transferCharacteristics;
            dstImage.matrixCoefficients = srcImage.matrixCoefficients;

            dstImage.transformFlags = srcImage.transformFlags;
            dstImage.pasp = srcImage.pasp;
            memcpy(&dstImage.pasp, &srcImage.pasp, sizeof(dstImage.pasp));
            memcpy(&dstImage.clap, &srcImage.clap, sizeof(dstImage.clap));
            memcpy(&dstImage.irot, &srcImage.irot, sizeof(dstImage.irot));
            memcpy(&dstImage.imir, &srcImage.imir, sizeof(dstImage.pasp));

avifImageSetProfileICC(dstImage, srcImage.icc.data, srcImage.icc.size);

avifImageSetMetadataExif(dstImage, srcImage.exif.data, srcImage.exif.size);
avifImageSetMetadataXMP(dstImage, srcImage.xmp.data, srcImage.xmp.size);

if ((planes & AVIF_PLANES_YUV) && srcImage.yuvPlanes[AVIF_CHAN_Y])
{
    avifImageAllocatePlanes(dstImage, AVIF_PLANES_YUV);

    avifPixelFormatInfo formatInfo;
    avifGetPixelFormatInfo(srcImage.yuvFormat, &formatInfo);
    uint32_t uvHeight = (dstImage.height + formatInfo.chromaShiftY) >> formatInfo.chromaShiftY;
    for (int yuvPlane = 0; yuvPlane < 3; ++yuvPlane)
    {
        uint32_t planeHeight = (yuvPlane == AVIF_CHAN_Y) ? dstImage.height : uvHeight;

        if (!srcImage.yuvRowBytes[yuvPlane])
        {
            // plane is absent. If we're copying from a source without
            // them, mimic the source image's state by removing our copy.
            avifFree(dstImage.yuvPlanes[yuvPlane]);
            dstImage.yuvPlanes[yuvPlane] = NULL;
            dstImage.yuvRowBytes[yuvPlane] = 0;
            continue;
        }

        for (uint32_t j = 0; j < planeHeight; ++j)
        {
            uint8_t* srcRow = &srcImage.yuvPlanes[yuvPlane][j * srcImage.yuvRowBytes[yuvPlane]];
            uint8_t* dstRow = &dstImage.yuvPlanes[yuvPlane][j * dstImage.yuvRowBytes[yuvPlane]];
            memcpy(dstRow, srcRow, dstImage.yuvRowBytes[yuvPlane]);
        }
    }
}

if ((planes & AVIF_PLANES_A) && srcImage.alphaPlane)
{
    avifImageAllocatePlanes(dstImage, AVIF_PLANES_A);
    for (uint32_t j = 0; j < dstImage.height; ++j)
    {
        uint8_t* srcAlphaRow = &srcImage.alphaPlane[j * srcImage.alphaRowBytes];
        uint8_t* dstAlphaRow = &dstImage.alphaPlane[j * dstImage.alphaRowBytes];
        memcpy(dstAlphaRow, srcAlphaRow, dstImage.alphaRowBytes);
    }
}
}

void avifImageDestroy(avifImage* image)
{
    avifImageFreePlanes(image, AVIF_PLANES_ALL);
    avifRWDataFree(&image.icc);
    avifRWDataFree(&image.exif);
    avifRWDataFree(&image.xmp);
    avifFree(image);
}

void avifImageSetProfileICC(avifImage* image, const uint8_t* icc, size_t iccSize)
{
    avifRWDataSet(&image.icc, icc, iccSize);
}

void avifImageSetMetadataExif(avifImage* image, const uint8_t* exif, size_t exifSize)
{
    avifRWDataSet(&image.exif, exif, exifSize);
}

void avifImageSetMetadataXMP(avifImage* image, const uint8_t* xmp, size_t xmpSize)
{
    avifRWDataSet(&image.xmp, xmp, xmpSize);
}

void avifImageAllocatePlanes(avifImage* image, uint32_t planes)
{
    int channelSize = avifImageUsesU16(image) ? 2 : 1;
    int fullRowBytes = channelSize * image.width;
    int fullSize = fullRowBytes * image.height;
    if ((planes & AVIF_PLANES_YUV) && (image.yuvFormat != avifPixelFormat.NONE))
    {
        avifPixelFormatInfo info;
        avifGetPixelFormatInfo(image.yuvFormat, &info);

        int shiftedW = (image.width + info.chromaShiftX) >> info.chromaShiftX;
        int shiftedH = (image.height + info.chromaShiftY) >> info.chromaShiftY;

        int uvRowBytes = channelSize * shiftedW;
        int uvSize = uvRowBytes * shiftedH;

        if (!image.yuvPlanes[AVIF_CHAN_Y])
        {
            image.yuvRowBytes[AVIF_CHAN_Y] = fullRowBytes;
            image.yuvPlanes[AVIF_CHAN_Y] = avifAlloc(fullSize);
        }

        if (image.yuvFormat != avifPixelFormat.YUV400)
        {
            if (!image.yuvPlanes[AVIF_CHAN_U])
            {
                image.yuvRowBytes[AVIF_CHAN_U] = uvRowBytes;
                image.yuvPlanes[AVIF_CHAN_U] = avifAlloc(uvSize);
            }
            if (!image.yuvPlanes[AVIF_CHAN_V])
            {
                image.yuvRowBytes[AVIF_CHAN_V] = uvRowBytes;
                image.yuvPlanes[AVIF_CHAN_V] = avifAlloc(uvSize);
            }
        }
        image.imageOwnsYUVPlanes = AVIF_TRUE;
    }
    if (planes & AVIF_PLANES_A)
    {
        if (!image.alphaPlane)
        {
            image.alphaRowBytes = fullRowBytes;
            image.alphaPlane = avifAlloc(fullSize);
        }
        image.imageOwnsAlphaPlane = AVIF_TRUE;
    }
}

void avifImageFreePlanes(avifImage* image, uint32_t planes)
{
    if ((planes & AVIF_PLANES_YUV) && (image.yuvFormat != avifPixelFormat.NONE))
    {
        if (image.imageOwnsYUVPlanes)
        {
            avifFree(image.yuvPlanes[AVIF_CHAN_Y]);
            avifFree(image.yuvPlanes[AVIF_CHAN_U]);
            avifFree(image.yuvPlanes[AVIF_CHAN_V]);
        }
        image.yuvPlanes[AVIF_CHAN_Y] = NULL;
        image.yuvRowBytes[AVIF_CHAN_Y] = 0;
        image.yuvPlanes[AVIF_CHAN_U] = NULL;
        image.yuvRowBytes[AVIF_CHAN_U] = 0;
        image.yuvPlanes[AVIF_CHAN_V] = NULL;
        image.yuvRowBytes[AVIF_CHAN_V] = 0;
        image.imageOwnsYUVPlanes = AVIF_FALSE;
    }
    if (planes & AVIF_PLANES_A)
    {
        if (image.imageOwnsAlphaPlane)
        {
            avifFree(image.alphaPlane);
        }
        image.alphaPlane = NULL;
        image.alphaRowBytes = 0;
        image.imageOwnsAlphaPlane = AVIF_FALSE;
    }
}

void avifImageStealPlanes(avifImage* dstImage, avifImage* srcImage, uint32_t planes)
{
    avifImageFreePlanes(dstImage, planes);

    if (planes & AVIF_PLANES_YUV)
    {
        dstImage.yuvPlanes[AVIF_CHAN_Y] = srcImage.yuvPlanes[AVIF_CHAN_Y];
        dstImage.yuvRowBytes[AVIF_CHAN_Y] = srcImage.yuvRowBytes[AVIF_CHAN_Y];
        dstImage.yuvPlanes[AVIF_CHAN_U] = srcImage.yuvPlanes[AVIF_CHAN_U];
        dstImage.yuvRowBytes[AVIF_CHAN_U] = srcImage.yuvRowBytes[AVIF_CHAN_U];
        dstImage.yuvPlanes[AVIF_CHAN_V] = srcImage.yuvPlanes[AVIF_CHAN_V];
        dstImage.yuvRowBytes[AVIF_CHAN_V] = srcImage.yuvRowBytes[AVIF_CHAN_V];

        srcImage.yuvPlanes[AVIF_CHAN_Y] = NULL;
        srcImage.yuvRowBytes[AVIF_CHAN_Y] = 0;
        srcImage.yuvPlanes[AVIF_CHAN_U] = NULL;
        srcImage.yuvRowBytes[AVIF_CHAN_U] = 0;
        srcImage.yuvPlanes[AVIF_CHAN_V] = NULL;
        srcImage.yuvRowBytes[AVIF_CHAN_V] = 0;

        dstImage.yuvFormat = srcImage.yuvFormat;
        dstImage.imageOwnsYUVPlanes = srcImage.imageOwnsYUVPlanes;
        srcImage.imageOwnsYUVPlanes = AVIF_FALSE;
    }
    if (planes & AVIF_PLANES_A)
    {
        dstImage.alphaPlane = srcImage.alphaPlane;
        dstImage.alphaRowBytes = srcImage.alphaRowBytes;

        srcImage.alphaPlane = NULL;
        srcImage.alphaRowBytes = 0;

        dstImage.imageOwnsAlphaPlane = srcImage.imageOwnsAlphaPlane;
        srcImage.imageOwnsAlphaPlane = AVIF_FALSE;
    }
}

avifBool avifImageUsesU16(const avifImage* image)
{
    return (image.depth > 8);
}

// avifCodecCreate*() functions are in their respective codec_*.c files

void avifCodecDestroy(avifCodec* codec)
{
    if (codec && codec.destroyInternal)
    {
        codec.destroyInternal(codec);
    }
    avifFree(codec);
}

// ---------------------------------------------------------------------------
// avifRGBImage

avifBool avifRGBFormatHasAlpha(avifRGBFormat format)
{
    return (format != AVIF_RGB_FORMAT_RGB) && (format != AVIF_RGB_FORMAT_BGR);
}

uint32_t avifRGBFormatChannelCount(avifRGBFormat format)
{
    return avifRGBFormatHasAlpha(format) ? 4 : 3;
}

uint32_t avifRGBImagePixelSize(const avifRGBImage* rgb)
{
    return avifRGBFormatChannelCount(rgb.format) * ((rgb.depth > 8) ? 2 : 1);
}

void avifRGBImageSetDefaults(avifRGBImage* rgb, const avifImage* image)
{
    rgb.width = image.width;
    rgb.height = image.height;
    rgb.depth = image.depth;
    rgb.format = AVIF_RGB_FORMAT_RGBA;
    rgb.chromaUpsampling = AVIF_CHROMA_UPSAMPLING_BILINEAR;
    rgb.ignoreAlpha = AVIF_FALSE;
    rgb.pixels = NULL;
    rgb.rowBytes = 0;
}

void avifRGBImageAllocatePixels(avifRGBImage* rgb)
{
    if (rgb.pixels)
    {
        avifFree(rgb.pixels);
    }

    rgb.rowBytes = rgb.width * avifRGBImagePixelSize(rgb);
    rgb.pixels = avifAlloc(rgb.rowBytes * rgb.height);
}

void avifRGBImageFreePixels(avifRGBImage* rgb)
{
    if (rgb.pixels)
    {
        avifFree(rgb.pixels);
    }

    rgb.pixels = NULL;
    rgb.rowBytes = 0;
}

// ---------------------------------------------------------------------------
// avifCodecSpecificOption

static char* avifStrdup(const char* str)
{
    size_t len = strlen(str);
    char* dup = avifAlloc(len + 1);
    memcpy(dup, str, len + 1);
    return dup;
}

avifCodecSpecificOptions* avifCodecSpecificOptionsCreate(void)
{
    avifCodecSpecificOptions* ava = avifAlloc(sizeof(avifCodecSpecificOptions));
    avifArrayCreate(ava, sizeof(avifCodecSpecificOption), 4);
    return ava;
}

void avifCodecSpecificOptionsDestroy(avifCodecSpecificOptions* csOptions)
{
    if (!csOptions)
    {
        return;
    }

    for (uint32_t i = 0; i < csOptions.count; ++i)
    {
        avifCodecSpecificOption* entry = &csOptions.entries[i];
        avifFree(entry.key);
        avifFree(entry.value);
    }
    avifArrayDestroy(csOptions);
    avifFree(csOptions);
}

void avifCodecSpecificOptionsSet(avifCodecSpecificOptions* csOptions, const char* key, const char* value)
{
    // Check to see if a key must be replaced
    for (uint32_t i = 0; i < csOptions.count; ++i)
    {
        avifCodecSpecificOption* entry = &csOptions.entries[i];
        if (!strcmp(entry.key, key))
        {
            if (value)
            {
                // Update the value
                avifFree(entry.value);
                entry.value = avifStrdup(value);
            }
            else
            {
                // Delete the value
                avifFree(entry.key);
                avifFree(entry.value);
                --csOptions.count;
                if (csOptions.count > 0)
                {
                    memmove(&csOptions.entries[i], &csOptions.entries[i + 1], (csOptions.count - i) * csOptions.elementSize);
                }
            }
            return;
        }
    }

    // Add a new key
    avifCodecSpecificOption* entry = (avifCodecSpecificOption*)avifArrayPushPtr(csOptions);
    entry.key = avifStrdup(key);
    entry.value = avifStrdup(value);
}

// ---------------------------------------------------------------------------
// Codec availability and versions

typedef const char* (*versionFunc)(void);
typedef avifCodec * (*avifCodecCreateFunc)(void);

struct AvailableCodec
{
    avifCodecChoice choice;
    const char* name;
    versionFunc version;
    avifCodecCreateFunc create;
    uint32_t flags;
};

// This is the main codec table; it determines all usage/availability in libavif.

static struct AvailableCodec availableCodecs[] = {
    // Ordered by preference (for AUTO)

#if defined(AVIF_CODEC_DAV1D)
    { AVIF_CODEC_CHOICE_DAV1D, "dav1d", avifCodecVersionDav1d, avifCodecCreateDav1d, AVIF_CODEC_FLAG_CAN_DECODE },
#endif
#if defined(AVIF_CODEC_LIBGAV1)
    { AVIF_CODEC_CHOICE_LIBGAV1, "libgav1", avifCodecVersionGav1, avifCodecCreateGav1, AVIF_CODEC_FLAG_CAN_DECODE },
#endif
#if defined(AVIF_CODEC_AOM)
    { AVIF_CODEC_CHOICE_AOM, "aom", avifCodecVersionAOM, avifCodecCreateAOM, AVIF_CODEC_FLAG_CAN_DECODE | AVIF_CODEC_FLAG_CAN_ENCODE },
#endif
#if defined(AVIF_CODEC_RAV1E)
    { AVIF_CODEC_CHOICE_RAV1E, "rav1e", avifCodecVersionRav1e, avifCodecCreateRav1e, AVIF_CODEC_FLAG_CAN_ENCODE },
#endif
    { AVIF_CODEC_CHOICE_AUTO, NULL, NULL, NULL, 0 }
};

static const int availableCodecsCount = (sizeof(availableCodecs) / sizeof(availableCodecs[0])) - 1;

static struct AvailableCodec *findAvailableCodec(avifCodecChoice choice, uint32_t requiredFlags)
{
    for (int i = 0; i < availableCodecsCount; ++i)
    {
        if ((choice != AVIF_CODEC_CHOICE_AUTO) && (availableCodecs[i].choice != choice))
        {
            continue;
        }
        if (requiredFlags && ((availableCodecs[i].flags & requiredFlags) != requiredFlags))
        {
            continue;
        }
        return &availableCodecs[i];
    }
    return NULL;
}

const char* avifCodecName(avifCodecChoice choice, uint32_t requiredFlags)
{
    struct AvailableCodec *availableCodec = findAvailableCodec(choice, requiredFlags);
if (availableCodec)
{
    return availableCodec.name;
}
return NULL;
}

avifCodecChoice avifCodecChoiceFromName(const char* name)
{
    for (int i = 0; i < availableCodecsCount; ++i)
    {
        if (!strcmp(availableCodecs[i].name, name))
        {
            return availableCodecs[i].choice;
        }
    }
    return AVIF_CODEC_CHOICE_AUTO;
}

avifCodec* avifCodecCreate(avifCodecChoice choice, uint32_t requiredFlags)
{
    struct AvailableCodec *availableCodec = findAvailableCodec(choice, requiredFlags);
if (availableCodec)
{
    return availableCodec.create();
}
return NULL;
}

static void append(char** writePos, size_t* remainingLen, const char* appendStr)
{
    size_t appendLen = strlen(appendStr);
    if (appendLen > *remainingLen)
    {
        appendLen = *remainingLen;
    }

    memcpy(*writePos, appendStr, appendLen);
    *remainingLen -= appendLen;
    *writePos += appendLen;
    *(*writePos) = 0;
}

void avifCodecVersions(char outBuffer[256])
{
    size_t remainingLen = 255;
    char* writePos = outBuffer;
    *writePos = 0;

    for (int i = 0; i < availableCodecsCount; ++i)
    {
        if (i > 0)
        {
            append(&writePos, &remainingLen, ", ");
        }
        append(&writePos, &remainingLen, availableCodecs[i].name);
        append(&writePos, &remainingLen, ":");
        append(&writePos, &remainingLen, availableCodecs[i].version());
    }
}
    }
}
