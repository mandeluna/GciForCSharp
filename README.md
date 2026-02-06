# GciForCSharp
C# FFI wrapper for the GemStone C Interface (GCI)

[GemStone](https://gemtalksystems.com/products/gs64/) is an object database and Smalltalk application runtime environment. You interact with the database through a dynamically linked C library available for Linux, macOS, and Windows. To use a C library from C# we use the built-in FFI.

GemBuilder for C documentation ([HTML](https://downloads.gemtalksystems.com/docs/GemStone64/3.7.x/GS64-GemBuilderC-3.7) or [PDF](https://downloads.gemtalksystems.com/docs/GemStone64/3.7.x/GS64-GemBuilderforC-3.7.pdf)) describes the API for the *single-threaded* GCI library. We are using a new *thread-safe* library that has fewer functions (but more features). It is not separately documented, but has a header file, `gcits.hf`, that is the definitive specification (a recent copy is included with this checkout).

The needed C libraries are not included as part of this checkout since there is a different set of libraries for each platform (Linux, macOS, and Windows), and for each GemStone version. You should download a recent version and the appropriate [product](https://gemtalksystems.com/products/gs64/) for your platform. Then move the appropriate files into the directory of your choice.

Heavily modified from James Foster's original implementation which had hard-coded platform-specific and version-specific class names and library dependencies. The GCI has been vastly simplified; we should be able to load the required libraries assuming that the `GEMSTONE` environment variable has been set.