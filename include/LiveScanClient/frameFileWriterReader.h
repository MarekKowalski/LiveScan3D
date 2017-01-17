#pragma once

#include <stdio.h>
#include <string>
#include <vector>
#include <chrono>
#include "utils.h"

class FrameFileWriterReader
{
public:
    FrameFileWriterReader();
	void openNewFileForWriting();
	void openCurrentFileForReading();
	
	// leave filename blank if you want the filename to be generated from the date
	void setCurrentFilename(std::string filename = ""); 

	void writeFrame(std::vector<Point3s> points, std::vector<RGB> colors);
	bool readFrame(std::vector<Point3s> &outPoints, std::vector<RGB> &outColors);

	bool openedForWriting() { return m_bFileOpenedForWriting; }
	bool openedForReading() { return m_bFileOpenedForReading; }


	void closeFileIfOpened();

    ~FrameFileWriterReader();

private:
	void resetTimer();
	int getRecordingTimeMilliseconds();

	FILE *m_pFileHandle = nullptr;
	bool m_bFileOpenedForWriting = false; 
	bool m_bFileOpenedForReading = false;

	std::string m_sFilename = "";

	std::chrono::steady_clock::time_point recording_start_time;
};