// stdafx.h : include file for standard system include files,
// or project specific include files that are used frequently, but
// are changed infrequently
#pragma once

// TODO: reference additional headers your program requires here
#define MAX_IMAGE_SIZE 262144
#define CLAMP(X,MIN,MAX) (X) < (MIN) ? (MIN) : ((X) > (MAX) ? (MAX) : (X))
#define SIGN(X) (X) < 0 ? -1 : ((X) > 0 ? 1 : 0)