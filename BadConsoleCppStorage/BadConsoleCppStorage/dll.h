#if defined(EXPORT_BUILD)
	#define DLL_EXPORT __declspec(dllexport)
#else
	#define DLL_EXPORT __declspec(dllimport)
#endif

extern "C" int DLL_EXPORT MyAdd(int x, int y);

struct CLogInfo
{
	const char* Content;
	int LogType;
	int CollapseCnt;
};

extern "C" int DLL_EXPORT GetLogStoreCnt();
extern "C" int DLL_EXPORT SaveLogStoreInfo(CLogInfo data);
extern "C" CLogInfo DLL_EXPORT GetLogStoreInfo(int idx);

extern "C" int DLL_EXPORT GetCollapsedLogCnt();
extern "C" int DLL_EXPORT PushCollapsedLog(CLogInfo data);
extern "C" int DLL_EXPORT SetCollapsedCount(int idx, int cnt);
extern "C" CLogInfo DLL_EXPORT GetCollapsedLog(int idx);


extern "C" int DLL_EXPORT PushCollapsedDict(const char* key, int v);
extern "C" int DLL_EXPORT GetCollapsedDictValue(const char* key);

extern "C" int DLL_EXPORT ClearAllStorage();

extern "C" int DLL_EXPORT GetFlagValue(int idx);
extern "C" int DLL_EXPORT SetFlagValue(int idx, int v);