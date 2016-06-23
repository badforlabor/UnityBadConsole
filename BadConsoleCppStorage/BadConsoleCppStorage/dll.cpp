#include <vector>
#include <map>
#include <string>

#define EXPORT_BUILD
#include "dll.h"

static int a = 0;

class Compare
{
public:
	bool operator()(const std::string& _Left, const std::string& _Right) const
	{		
		return strcmp(_Left.c_str(), _Right.c_str()) < 0;
	}
};
static std::map<std::string, int, Compare> CollapsedLogDict;

int DLL_EXPORT MyAdd(int x, int y)
{
	a++;
	//return a;
	return CollapsedLogDict.size();
}

static void ResetValue(CLogInfo& data)
{
	size_t size = strlen(data.Content);
	char* c = new char[size + 1];
	strcpy(c, data.Content);
	data.Content = c;
}

static std::vector<CLogInfo> LogStore;
extern "C" int DLL_EXPORT GetLogStoreCnt()
{
	return LogStore.size();
}
extern "C" int DLL_EXPORT SaveLogStoreInfo(CLogInfo data)
{
	ResetValue(data);
	LogStore.push_back(data);
	return 0;
}
extern "C" CLogInfo DLL_EXPORT GetLogStoreInfo(int idx)
{
	return LogStore[idx];
}

static std::vector<CLogInfo> CollapsedLogStore;
extern "C" int DLL_EXPORT GetCollapsedLogCnt()
{
	return CollapsedLogStore.size();
}
extern "C" int DLL_EXPORT PushCollapsedLog(CLogInfo data)
{
	ResetValue(data);
	CollapsedLogStore.push_back(data);
	return 0;
}
extern "C" CLogInfo DLL_EXPORT GetCollapsedLog(int idx)
{
	return CollapsedLogStore[idx];
}
extern "C" int DLL_EXPORT SetCollapsedCount(int idx, int cnt)
{
	CollapsedLogStore[idx].CollapseCnt = cnt;
	return 0;
}

extern "C" int DLL_EXPORT PushCollapsedDict(const char* key, int v)
{
	std::string skey = (key);
	CollapsedLogDict[skey] = v;
	return 0;
}
extern "C" int DLL_EXPORT GetCollapsedDictValue(const char* key)
{
	std::string skey = (key);
	if (CollapsedLogDict.find(skey) != CollapsedLogDict.end())
	{
		return CollapsedLogDict[skey];
	}
	return 0;
}

extern "C" int DLL_EXPORT ClearAllStorage()
{
	CollapsedLogDict.clear();
	CollapsedLogStore.clear();
	LogStore.clear();

	return 0;
}
static int FlagValue = 116;	// 1110100
extern "C" int DLL_EXPORT GetFlagValue(int idx)
{
	return ((1 << idx) & FlagValue) > 0 ? 1 : 0;
}
extern "C" int DLL_EXPORT SetFlagValue(int idx, int v)
{
	if (v > 0)
	{
		if (GetFlagValue(idx) == 0)
		{
			FlagValue = FlagValue ^ ((1 << idx));
		}
	}
	else
	{
		FlagValue = FlagValue & (~(1 << idx));
	}
	return 0;
}