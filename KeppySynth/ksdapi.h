// KSDAPI calls

void keepstreamsalive(int& opend) {
	BASS_ChannelIsActive(KSStream);
	if (BASS_ErrorGetCode() == 5 || livechange) {
		PrintToConsole(FOREGROUND_RED, 1, "Restarting audio stream...");
		CloseThreads();
		LoadSettings(TRUE);
		if (!com_initialized) { if (!FAILED(CoInitialize(NULL))) com_initialized = TRUE; }
		SetConsoleTextAttribute(hConsole, FOREGROUND_RED);
		if (InitializeBASS(FALSE)) {
			SetUpStream();
			LoadSoundFontsToStream();
			opend = CreateThreads(TRUE);
		}
	}
}

DWORD WINAPI threadfunc(LPVOID lpV) {
	try {
		if (BannedSystemProcess() == TRUE) {
			_endthread();
			return 0;
		}
		else {
			int opend = 0;
			while (opend == 0) {
				LoadSettings(FALSE);
				allocate_memory();
				load_bassfuncs();
				if (!com_initialized) {
					if (FAILED(CoInitialize(NULL))) continue;
					com_initialized = TRUE;
				}
				SetConsoleTextAttribute(hConsole, FOREGROUND_RED);
				if (InitializeBASS(FALSE)) {
					SetUpStream();
					LoadSoundFontsToStream();
					opend = CreateThreads(TRUE);
				}
			}
			PrintToConsole(FOREGROUND_RED, 1, "Checking for settings changes or hotkeys...");
			while (stop_rtthread == FALSE) {
				start1 = TimeNow();
				keepstreamsalive(opend);
				LoadCustomInstruments();
				CheckVolume();
				ParseDebugData();
				Sleep(10);
			}
			stop_rtthread = FALSE;
			FreeUpLibraries();
			PrintToConsole(FOREGROUND_RED, 1, "Closing main thread...");
			ExitThread(0);
			return 0;
		}
	}
	catch (...) {
		CrashMessage(L"DrvMainThread");
		ExitThread(0);
		throw;
		return 0;
	}
}

void DoStartClient() {
	if (modm_closed == TRUE) {
		timeBeginPeriod(1);
		InitializeCriticalSection(&bufmed);

		HKEY hKey;
		long lResult;
		DWORD dwType = REG_DWORD;
		DWORD dwSize = sizeof(DWORD);
		int One = 0;
		lResult = RegOpenKeyEx(HKEY_CURRENT_USER, L"Software\\Keppy's Synthesizer", 0, KEY_ALL_ACCESS, &hKey);
		RegQueryValueEx(hKey, L"driverprio", NULL, &dwType, (LPBYTE)&driverprio, &dwSize);
		RegCloseKey(hKey);
		lResult = RegOpenKeyEx(HKEY_CURRENT_USER, L"Software\\Keppy's Synthesizer\\Settings", 0, KEY_ALL_ACCESS, &hKey);
		RegQueryValueEx(hKey, L"hypermode", NULL, &dwType, (LPBYTE)&HyperMode, &dwSize);
		RegCloseKey(hKey);

		if (HyperMode && !HyperCheckedAlready) {
			MessageBox(NULL, L"Hyper-playback mode is enabled!", L"Keppy's Synthesizer", MB_ICONWARNING | MB_OK | MB_SYSTEMMODAL);
			_PrsData = ParseDataHyper;
			_PlayBufData = PlayBufferedDataHyper;
		}
		else {
			_PrsData = ParseData;
			_PlayBufData = PlayBufferedData;
		}
		HyperCheckedAlready = TRUE;

		AppName();
		StartDebugPipe(FALSE);

		DWORD result;
		processPriority = GetPriorityClass(GetCurrentProcess());
		SetPriorityClass(GetCurrentProcess(), NORMAL_PRIORITY_CLASS);
		load_sfevent = CreateEvent(
			NULL,               // default security attributes
			TRUE,               // manual-reset event
			FALSE,              // initial state is nonsignaled
			TEXT("SoundFontEvent")  // object name
		);
		hCalcThread = CreateThread(NULL, 0, (LPTHREAD_START_ROUTINE)threadfunc, NULL, 0, (LPDWORD)thrdaddrC);
		SetThreadPriority(hCalcThread, prioval[driverprio]);
		result = WaitForSingleObject(load_sfevent, INFINITE);
		if (result == WAIT_OBJECT_0)
		{
			CloseHandle(load_sfevent);
		}
		modm_closed = FALSE;
	}
}

void DoStopClient() {
	if (modm_closed == FALSE) {
		stop_thread = TRUE;
		stop_rtthread = TRUE;
		WaitForSingleObject(hCalcThread, INFINITE);
		CloseHandle(hCalcThread);
		modm_closed = TRUE;
		SetPriorityClass(GetCurrentProcess(), processPriority);
		DeleteCriticalSection(&bufmed);
		timeEndPeriod(1);
	}
}

void DoResetClient() {
	ResetSynth(0);
}

char const* WINAPI ReturnKSDAPIVer() {
	return "v1.11 (Release)";
}

BOOL WINAPI IsKSDAPIAvailable()  {
	HKEY hKey;
	long lResult;
	DWORD dwType = REG_DWORD;
	DWORD dwSize = sizeof(DWORD);
	lResult = RegOpenKeyEx(HKEY_CURRENT_USER, L"Software\\Keppy's Synthesizer\\Settings", 0, KEY_READ, &hKey);
	RegQueryValueEx(hKey, L"allowksdapi", NULL, &dwType, (LPBYTE)&ksdirectenabled, &dwSize);
	RegCloseKey(hKey);

	return ksdirectenabled;
}

void InitializeKSStream() {
	DoStartClient();
}

void TerminateKSStream() {
	DoStopClient();
}

void ResetKSStream() {
	ResetSynth(0);
}

MMRESULT WINAPI SendDirectData(DWORD dwMsg) {
	return _PrsData(MODM_DATA, dwMsg, 0, 0, 0);
}

MMRESULT WINAPI SendDirectDataNoBuf(DWORD dwMsg) {
	try {
		if (bufferinitialized) SendToBASSMIDI(dwMsg);
		return MMSYSERR_NOERROR;
	}
	catch (...) { return MMSYSERR_INVALPARAM; }
}

MMRESULT WINAPI SendDirectLongData(MIDIHDR* IIMidiHdr) {
	try {
		if (bufferinitialized) {
			DWORD exlen;
			unsigned char* sysexbuffer;
			DWORD retval = MMSYSERR_NOERROR;
			if (!(IIMidiHdr->dwFlags & MHDR_PREPARED)) return MIDIERR_UNPREPARED;

			// Mark the buffer as in queue
			IIMidiHdr->dwFlags &= ~MHDR_DONE;
			IIMidiHdr->dwFlags |= MHDR_INQUEUE;

			// Do the stuff with it, if it's not to be ignored
			if (!sysexignore)
			{
				// Allocate temp buffer for the event
				exlen = IIMidiHdr->dwBytesRecorded;
				sysexbuffer = (unsigned char *)malloc(exlen * sizeof(char));
				if (sysexbuffer)
				{
					try
					{
						// Copy the event, and send it over to the events parser
						memcpy(sysexbuffer, IIMidiHdr->lpData, exlen);
						retval = ParseData(MODM_LONGDATA, 0, 0, exlen, sysexbuffer);
					}
					catch (...)
					{
						// Something happened, return "Invalid parameter"
						retval = MMSYSERR_INVALPARAM;
					}
				}
				// The buffer is invalid, return "No memory"
				else retval = MMSYSERR_NOMEM;
			}
			// It has to be ignored, send info to console
			else PrintToConsole(FOREGROUND_RED, (DWORD)IIMidiHdr->lpData, "Ignored SysEx MIDI event.");

			// Mark the buffer as done
			IIMidiHdr->dwFlags &= ~MHDR_INQUEUE;
			IIMidiHdr->dwFlags |= MHDR_DONE;

			// Tell the app that the buffer has been played
			return retval;
		}
		else return MIDIERR_NOTREADY;
	}
	catch (...) { return MMSYSERR_INVALPARAM; }
}

MMRESULT WINAPI SendDirectLongDataNoBuf(MIDIHDR* IIMidiHdr) {
	try {
		if (bufferinitialized) {
			DWORD exlen;
			unsigned char* sysexbuffer;
			DWORD retval = MMSYSERR_NOERROR;
			if (!(IIMidiHdr->dwFlags & MHDR_PREPARED)) return MIDIERR_UNPREPARED;

			// Mark the buffer as in queue
			IIMidiHdr->dwFlags &= ~MHDR_DONE;
			IIMidiHdr->dwFlags |= MHDR_INQUEUE;

			// Do the stuff with it, if it's not to be ignored
			if (!sysexignore)
			{
				// Allocate temp buffer for the event
				exlen = IIMidiHdr->dwBytesRecorded;
				sysexbuffer = (unsigned char *)malloc(exlen * sizeof(char));
				if (sysexbuffer)
				{
					try
					{
						// Copy the event, and send it over to the events parser
						memcpy(sysexbuffer, IIMidiHdr->lpData, exlen);
						SendLongToBASSMIDI(sysexbuffer, exlen);
					}
					catch (...)
					{
						// Something happened, return "Invalid parameter"
						retval = MMSYSERR_INVALPARAM;
					}
				}
				// The buffer is invalid, return "No memory"
				else retval = MMSYSERR_NOMEM;
			}
			// It has to be ignored, send info to console
			else PrintToConsole(FOREGROUND_RED, (DWORD)IIMidiHdr->lpData, "Ignored SysEx MIDI event.");

			// Mark the buffer as done
			IIMidiHdr->dwFlags &= ~MHDR_INQUEUE;
			IIMidiHdr->dwFlags |= MHDR_DONE;

			// Tell the app that the buffer has been played
			return retval;
		}
		else return MIDIERR_NOTREADY;
	}
	catch (...) { return MMSYSERR_INVALPARAM; }
}