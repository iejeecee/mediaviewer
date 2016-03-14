#pragma once

namespace VideoLib {

public enum class MediaType
{
	UNKNOWN_MEDIA,
	IMAGE_MEDIA,
	VIDEO_MEDIA,
	AUDIO_MEDIA

};

public enum class SeekMode
{
	SEEK_BY_PTS,
	SEEK_BY_DTS
};

}