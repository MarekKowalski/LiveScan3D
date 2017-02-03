#include "frameFileWriterReader.h"

#include <ctime>

FrameFileWriterReader::FrameFileWriterReader()
{

}

void FrameFileWriterReader::closeFileIfOpened()
{
	if (m_pFileHandle == nullptr)
		return;

	fclose(m_pFileHandle);
	m_pFileHandle = nullptr; 
	m_bFileOpenedForReading = false;
	m_bFileOpenedForWriting = false;
}

void FrameFileWriterReader::resetTimer()
{
	recording_start_time = std::chrono::steady_clock::now();
}

int FrameFileWriterReader::getRecordingTimeMilliseconds()
{
	std::chrono::steady_clock::time_point end = std::chrono::steady_clock::now();
	return static_cast<int>(std::chrono::duration_cast<std::chrono::milliseconds >(end - recording_start_time).count());
}

void FrameFileWriterReader::openCurrentFileForReading()
{
	closeFileIfOpened();

	m_pFileHandle = fopen(m_sFilename.c_str(), "rb");

	m_bFileOpenedForReading = true;
	m_bFileOpenedForWriting = false;
}

void FrameFileWriterReader::openNewFileForWriting()
{
	closeFileIfOpened();

	char filename[1024];
	time_t t = time(0);
	struct tm * now = localtime(&t);
	sprintf(filename, "recording_%04d_%02d_%02d_%02d_%02d.bin", now->tm_year + 1900, now->tm_mon + 1, now->tm_mday, now->tm_hour, now->tm_min, now->tm_sec);
	m_sFilename = filename; 
	m_pFileHandle = fopen(filename, "wb");

	m_bFileOpenedForReading = false;
	m_bFileOpenedForWriting = true;

	resetTimer();
}

bool FrameFileWriterReader::readFrame(std::vector<Point3s> &outPoints, std::vector<RGB> &outColors)
{
	if (!m_bFileOpenedForReading)
		openCurrentFileForReading();

	outPoints.clear();
	outColors.clear();
	FILE *f = m_pFileHandle;
	int nPoints, timestamp; 
	char tmp[1024]; 
	int nread = fscanf_s(f, "%s %d %s %d", tmp, 1024, &nPoints, tmp, 1024, &timestamp);

	if (nread < 4)
		return false;

	if (nPoints == 0)
		return true;

	fgetc(f);		//  '\n'
	outPoints.resize(nPoints);
	outColors.resize(nPoints);

	fread((void*)outPoints.data(), sizeof(outPoints[0]), nPoints, f);
	fread((void*)outColors.data(), sizeof(outColors[0]), nPoints, f);
	fgetc(f);		// '\n'
	return true;

}


void FrameFileWriterReader::writeFrame(std::vector<Point3s> points, std::vector<RGB> colors)
{
	if (!m_bFileOpenedForWriting)
		openNewFileForWriting();

	FILE *f = m_pFileHandle;

	int nPoints = static_cast<int>(points.size());
	fprintf(f, "n_points= %d\nframe_timestamp= %d\n", nPoints, getRecordingTimeMilliseconds());
	if (nPoints > 0)
	{
		fwrite((void*)points.data(), sizeof(points[0]), nPoints, f);
		fwrite((void*)colors.data(), sizeof(colors[0]), nPoints, f);
	}
	fprintf(f, "\n");
}

FrameFileWriterReader::~FrameFileWriterReader()
{
	closeFileIfOpened();
}