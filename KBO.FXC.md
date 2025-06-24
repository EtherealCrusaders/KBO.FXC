
## Includes 

### WIP THE CURRENT BEHAVIOUR MAY NOT BE THE SAME 

TLDR; the behaviour is the same as [in C/C++](https://learn.microsoft.com/en-us/cpp/preprocessor/hash-include-directive-c-cpp?view=msvc-170) except for the environment variables.

For a #include directive with quotes (`#include "file.h"`) files are searched in this order:
1. In the same directory as the file that contains the #include statement.
2. In the directories of the currently opened include files, in the reverse order in which they were opened. The search begins in the directory of the parent include file and continues upward through the directories of any grandparent include files.
3. Along the path that's specified by each -I compiler option or the `KBOEffectsIncludePaths` msbuild property. 


For a #include directive with angle brackets (`#include <file.h>`) files are searched in this order:
1. Along the path that's specified by each -I compiler option or the `KBOEffectsIncludePaths` msbuild property. 