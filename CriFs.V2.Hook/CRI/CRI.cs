using System.Runtime.InteropServices;
using Function64 = Reloaded.Hooks.Definitions.X64.FunctionAttribute;
using Function32 = Reloaded.Hooks.Definitions.X86.FunctionAttribute;
using CallConv64 = Reloaded.Hooks.Definitions.X64.CallingConventions;
using CallConv32 = Reloaded.Hooks.Definitions.X86.CallingConventions;

// ReSharper disable InconsistentNaming
// ReSharper disable EnumUnderlyingTypeIsInt

namespace CriFs.V2.Hook.CRI;

// Documentation taken from publicly available CRI SDK
public static unsafe class CRI
{
    /// <summary>
    /// Initializes the CRI File System.
    /// </summary>
    /// <param name="config">[in] Bind ID.</param>
    /// <param name="buffer">[in] Buffer/work area passed to function.</param>
    /// <param name="size">[in] Size of the work area.</param>
    /// <returns>CriError Error code.</returns>
    /// <remarks>
    ///     This function must be called before calling any of the other CRI functions.
    /// </remarks>
    [Function64(CallConv64.Microsoft)]
    [Function32(CallConv32.Cdecl)]
    public delegate CriError criFs_InitializeLibrary(CriFsConfig* config, void* buffer, int size);
    
    /// <summary>
    /// Finalizes (i.e. Disposes) the CRI File System.
    /// </summary>
    /// <returns>CriError Error code.</returns>
    /// <remarks>
    ///     You cannot call any CRI functions other than <see cref="criFs_InitializeLibrary"/> after calling this.
    /// </remarks>
    [Function64(CallConv64.Microsoft)]
    [Function32(CallConv32.Cdecl)]
    public delegate CriError criFs_FinalizeLibrary();
    
    /// <summary>
    /// Work area size needed for the CRI library.
    /// </summary>
    /// <param name="config">[in] Configuration pointer.</param>
    /// <param name="numBytes">[out] Pointer to where final work area size will be stored.</param>
    /// <returns>Error code.</returns>
    [Function64(CallConv64.Microsoft)]
    [Function32(CallConv32.Cdecl)]
    public delegate CriError criFs_CalculateWorkSizeForLibrary(CriFsConfig* config, int* numBytes);
    
    /// <summary>
    /// Get the bind status. 
    /// </summary>
    /// <param name="bndrid">[in] Bind ID.</param>
    /// <param name="status">[out] Binder status. </param>
    /// <returns>CriError Error code.</returns>
    [Function64(CallConv64.Microsoft)]
    [Function32(CallConv32.Cdecl)]
    public delegate CriError criFsBinder_GetStatus(uint bndrid, CriFsBinderStatus* status);

    /// <summary>
    /// Bind the CPK file. 
    /// </summary>
    /// <param name="bndrhn">Binder handle of the bind destination.</param>
    /// <param name="srcbndrhn">Binder handle to access the CPK file to bind.</param>
    /// <param name="path">Path name of the CPK file to bind.</param>
    /// <param name="work">Work area for bind (mainly for CPK analysis).</param>
    /// <param name="worksize">Size of the work area (bytes).</param>
    /// <param name="bndrid">[out] Bind ID.</param>
    /// <returns>CriError Error code.</returns>
    [Function64(CallConv64.Microsoft)]
    [Function32(CallConv32.Cdecl)]
    public delegate CriError criFsBinder_BindCpk(IntPtr bndrhn, IntPtr srcbndrhn,
        [MarshalAs(UnmanagedType.LPStr)] string path, IntPtr work, int worksize, uint* bndrid);

    /// <summary>
    /// Bind the given list of files
    /// </summary>
    /// <param name="bndrhn">Binder handle of the bind destination.</param>
    /// <param name="srcbndrhn">Binder handle to access the CPK file to bind.</param>
    /// <param name="path">List of files to bind, with `\n` as separator.</param>
    /// <param name="work">Work area for bind (mainly for CPK analysis).</param>
    /// <param name="worksize">Size of the work area (bytes).</param>
    /// <param name="bndrid">[out] Bind ID.</param>
    /// <returns>CriError Error code.</returns>
    [Function64(CallConv64.Microsoft)]
    [Function32(CallConv32.Cdecl)]
    public delegate CriError criFsBinder_BindFiles(IntPtr bndrhn, IntPtr srcbndrhn,
        [MarshalAs(UnmanagedType.LPStr)] string path, IntPtr work, int worksize, uint* bndrid);
    
    /// <summary>
    /// Bind the given list of files
    /// </summary>
    /// <param name="bndrhn">Binder handle of the bind destination.</param>
    /// <param name="srcbndrhn">Binder handle to access the CPK file to bind.</param>
    /// <param name="path">List of files to bind, with `\n` as separator.</param>
    /// <param name="work">Work area for bind (mainly for CPK analysis).</param>
    /// <param name="worksize">Size of the work area (bytes).</param>
    /// <param name="bndrid">[out] Bind ID.</param>
    /// <returns>CriError Error code.</returns>
    [Function64(CallConv64.Microsoft)]
    [Function32(CallConv32.Cdecl)]
    public delegate CriError criFsBinder_BindFiles_WithoutMarshalling(IntPtr bndrhn, IntPtr srcbndrhn,
        byte* path, IntPtr work, int worksize, uint* bndrid);
    
    // In SonicGenerations bndrhn seems unused.
    
    /// <summary>
    /// Finds a file that's attached to a given binder.
    /// </summary>
    /// <param name="bndrhn">[In] Binder handle of the bind destination.</param>
    /// <param name="path">[In] Path to the file in question.</param>
    /// <param name="finfo">[Out] File info, unknown format.</param>
    /// <param name="exist">[Out] True if file exists, else false.</param>
    [Function64(CallConv64.Microsoft)]
    [Function32(CallConv32.Cdecl)]
    public delegate nint criFsBinder_Find(nint bndrhn, nint path, CriFsBinderFileInfo* finfo, int* exist);
    
    /// <summary>
    /// This function sets the priority value for the bind ID. 
    /// Using the priority enables you to control the order of searching the bind IDs in a binder handle. 
    /// The priority value of the ID is 0 when bound, and IDs are searched in the binding order of them with the same priority. 
    /// The larger the priority value is, the higher the priority with higher search order is. 
    /// </summary>
    /// <param name="bndrid">Bind ID.</param>
    /// <param name="priority">Priority value.</param>
    /// <returns>CriError Error code.</returns>
    [Function64(CallConv64.Microsoft)]
    [Function32(CallConv32.Cdecl)]
    public delegate CriError criFsBinder_SetPriority(uint bndrid, int priority);

    /// <summary>
    /// Delete bind ID (Unbind): Blocking function. 
    /// </summary>
    /// <param name="bndrid">Bind ID.</param>
    /// <returns>CriError Error code.</returns>
    [Function64(CallConv64.Microsoft)]
    [Function32(CallConv32.Cdecl)]
    public delegate CriError criFsBinder_Unbind(uint bndrid);
    
    /// <summary>
    /// Get allocation size needed for work directory.
    /// </summary>
    /// <param name="srcbndrhn">Binder handle to access directory to bind.</param>
    /// <param name="path">List of files to bind. '\n' as delimiter.</param>
    /// <param name="workSize">Necessary work size.</param>
    /// <returns>CriError Error code.</returns>
    [Function64(CallConv64.Microsoft)]
    [Function32(CallConv32.Cdecl)]
    public delegate CriError criFsBinder_GetWorkSizeForBindFiles(IntPtr srcbndrhn,
        [MarshalAs(UnmanagedType.LPStr)] string path, int* workSize);
    
    // !! Internal Functions !! NON PUBLIC API
    
    /// <summary>
    /// Registers a file before loading it.
    /// </summary>
    /// <param name="loader">The handle to the CriFs Loader.</param>
    /// <param name="binder">The handle to the CriFs Binder.</param>
    /// <param name="path">
    ///     Pointer to a string with the file path.
    ///     This path is relative and is usually ANSI.
    /// </param>
    /// <param name="fileId">The ID of the file within the archive (CPK). -1 if not using ID.</param>
    /// <param name="zero">Unknown, usually zero.</param>
    [Function64(CallConv64.Microsoft)]
    [Function32(Function32.Register.esi, Function32.Register.eax, Function32.StackCleanup.Caller)] // Always optimized as this due to simplicity of caller & register allocation pattern under MSVC
    public delegate IntPtr criFsLoader_RegisterFile(IntPtr loader,
        IntPtr binder,
        IntPtr path,
        int fileId,
        IntPtr zero);
    
    /// <summary>
    /// Verifies whether a given file exists on the filesystem.
    /// </summary>
    /// <param name="result">The result of the operation is stored here.</param>
    /// <param name="stringPtr">
    ///     Pointer to a string with the file path.
    ///     This path is usually relative, and can either be ANSI or UTF-8.
    ///
    ///     If it's UTF-8, you must pass it to `MultiByteToWideChar` to convert to wide string.
    /// </param>
    [Function64(CallConv64.Microsoft)]
    [Function32(CallConv32.Cdecl)]
    public delegate IntPtr criFsIo_Exists(byte* stringPtr, int* result);
    
    // Delete & Rename not implemented, mods should be immutable from regular game code.
    
    /// <summary>
    /// Verifies whether a given file exists on the filesystem.
    /// </summary>
    /// <param name="stringPtr">
    ///     Pointer to a string with the file path.
    ///     This path is usually relative, and can either be ANSI or UTF-8.
    ///
    ///     If it's UTF-8, you must pass it to `MultiByteToWideChar` to convert to wide string.
    /// </param>
    /// <param name="fileCreationType">Behaviour of the file opening. Current values are unknown.</param>
    /// <param name="desiredAccess">Required file access. Current values are unknown.</param>
    /// <param name="result">Result of the operation.</param>
    [Function64(CallConv64.Microsoft)]
    [Function32(CallConv32.Cdecl)]
    public delegate IntPtr criFsIo_Open(byte *stringPtr, int fileCreationType, int desiredAccess, nint** result);
    
    /// <summary>
    /// Status of the CRI binder.
    /// </summary>
    public enum CriFsBinderStatus : int
    {
        CRIFSBINDER_STATUS_NONE = 0,

        /// <summary>Binding.</summary>
        CRIFSBINDER_STATUS_ANALYZE,

        /// <summary>Bound.</summary>
        CRIFSBINDER_STATUS_COMPLETE,

        /// <summary>Unbinding.</summary>
        CRIFSBINDER_STATUS_UNBIND,

        /// <summary>Unbound.</summary>
        CRIFSBINDER_STATUS_REMOVED,

        /// <summary>Invalid Bind.</summary>
        CRIFSBINDER_STATUS_INVALID,

        /// <summary>Bind Failed.</summary>
        CRIFSBINDER_STATUS_ERROR
    }

    public enum CriError : int
    {
        /// <summary>Succeeded.</summary>
        CRIERR_OK = 0,

        /// <summary>Error occurred.</summary>
        CRIERR_NG = -1,

        /// <summary>Invalid argument.</summary>
        CRIERR_INVALID_PARAMETER = -2,

        /// <summary>Failed to allocate memory.</summary>
        CRIERR_FAILED_TO_ALLOCATE_MEMORY = -3,

        /// <summary>Parallel execution of thread-unsafe function.</summary>
        CRIERR_UNSAFE_FUNCTION_CALL = -4,

        /// <summary>Function not implemented.</summary>
        CRIERR_FUNCTION_NOT_IMPLEMENTED = -5,

        /// <summary>Library not initialized.</summary>
        CRIERR_LIBRARY_NOT_INITIALIZED = -6,
    }

    /// <summary>
    /// Configuration for the CRI File System library.
    /// </summary>
    /// <remarks>
    /// This struct specifies the configuration for initializing the CRI File System library.
    /// Pass it to the <see cref="criFs_InitializeLibrary"/> method.
    /// 
    /// The library allocates internal resources based on this config.
    /// Using smaller values reduces memory usage but may cause handle allocation to fail if too small.
    ///
    /// From my observation, most games use the default values.
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    public struct CriFsConfig
    {
        /// <summary>
        /// The thread model for the CRI File System library.
        /// </summary>
        public CriFsThreadModel ThreadModel;

        /// <summary>
        /// The max number of binder instances that can be simultaneously allocated.
        /// </summary>
        public int MaxBinders;

        /// <summary>
        /// The max number of loader instances that can be simultaneously allocated.
        /// </summary>
        public int MaxLoaders;

        /// <summary>
        /// The max number of group loader instances that can be simultaneously allocated.
        /// </summary>
        public int MaxGroupLoaders;

        /// <summary>
        /// The max number of CRI stdio (CRI abstraction over STD IO ??) that can be simultaneously allocated.
        /// </summary>
        public int MaxStdioHandles;

        /// <summary>
        /// The max number of installer (??) instances that can be simultaneously allocated.
        /// </summary>
        public int MaxInstallers;

        /// <summary>
        /// The max number of simultaneous bind operations.
        /// </summary>
        /// <remarks>
        /// For example if <see cref="criFsBinder_BindCpk"/> and <see cref="criFsBinder_Unbind"/>
        /// are called alternating, set this to 1, otherwise set it to max amount of alive binds at any given time.
        /// </remarks>
        public int MaxBinds;

        /// <summary>
        /// The max number of files to open simultaneously.
        /// </summary>
        /// <remarks>
        /// Set this to the max number of files opened concurrently.
        ///
        /// The library opens files during:
        /// - Binding CPKs/files
        /// - Loading files
        /// 
        /// So set MaxFiles to the max files opened across all these operations.
        ///
        /// When using ADX library via bridge lib, ADXT/CriSsPly handles use CriFsStdio handles internally.
        /// So when using bridge lib, set this to: 
        /// MaxFiles = MaxStdioHandles + MaxADXTHandles + MaxCriSsPlyHandles + other files
        /// </remarks>
        public int MaxFiles;

        /// <summary>
        /// The max file path length in bytes.
        /// </summary>
        /// <remarks>
        /// Set this to the longest file path used in the game, including null terminator.
        /// So if a 256 character path is used, set this to 256 even if other paths are shorter.
        ///
        /// The length should include the null terminator byte.
        ///
        /// For user installable apps, use the max expected user path length.
        /// </remarks>
        public int MaxPath;

        /// <summary>
        /// The CRI File System library version.
        /// </summary>
        public uint Version;

        /// <summary>
        /// Whether to validate CPK files using CRC.
        /// </summary>
        public int EnableCrcCheck;
    }

    /// <summary>
    /// Threading models for the CRI File System library.
    /// </summary>
    /// <remarks>
    /// Specify the desired model in <see cref="CriFsConfig.ThreadModel"/>.
    /// </remarks>
    /// <seealso cref="CriFsConfig.ThreadModel"/>
    public enum CriFsThreadModel : int // <= signed int32
    {
        /// <summary>
        /// Multithreaded model.
        /// The library creates threads internally.
        /// </summary>
        MultiThreaded = 0,

        /// <summary>
        /// Multithreaded model driven by user calls.
        /// The library creates threads internally, but processing only runs when `criFs_ExecuteMain` is called.
        /// </summary>
        MultiThreadedUserDriven = 3,

        /// <summary>
        /// User must handle threading explicitly.
        /// </summary>    
        UserMultiThreaded = 1,

        /// <summary>
        /// Single threaded model.
        /// </summary>
        SingleThreaded = 2,
    }
    
    [StructLayout(LayoutKind.Sequential)]
    public struct CriFsBinderFileInfo 
    {
        /// <summary>
        /// File handle.
        /// </summary>
        public void* fileHn;
        
        /// <summary>
        /// Path to the file, ANSI, null terminated.
        /// </summary>
        public byte* Path;
        
        /// <summary>
        /// Offset position from beginning of file.
        /// </summary>
        public long Offset;
        
        /// <summary>
        /// Compressed file size.
        /// </summary>
        public long CompressedSize;
        
        /// <summary>
        /// Uncompressed file size.
        /// </summary>
        public long DecompressedSize;
        
        /// <summary>
        /// Binder ID (Indicates binder from which file is bound)
        /// </summary>
        public void* binderId;

        /// <summary>
        /// Reserved/padding.
        /// </summary>
        public uint Reserved;
    }
}