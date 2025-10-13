<p align="center">
<img src="https://github.com/chkr1011/CoAPnet/blob/master/Images/icon_det_512.png?raw=true" width="196">
<br/>
<br/>
</p>

[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](https://raw.githubusercontent.com/chkr1011/CoAPnet/master/LICENSE)

# Matizk.CoAP

CoAPnet is a high performance .NET library for CoAPnet based communication. The library compiled for .NET9

## Features

### Protocol

* Core protocol (RFC 7252)
* Block transfer (RFC 7959) - Complete (Block1 & Block2) with early and late negotiation
* Observe (RFC 7641) - _Not implemented yet_

### General

* Async support
* DTLS (up to 1.2) support
* TLS 1.2 support
* Extensible communication channels (e.g. In-Memory, TCP, TCP+TLS, UDP, UDP+TLS)
* Lightweight (only the low level implementation of CoAPnet, no overhead)
* Performance optimized
* Interfaces included for mocking and testing
* Access to internal trace messages
* Unit tested
* No external dependencies

### Client

* Communication via TCP (+TLS) or UDP (+DTLS) supported
* Included core _LowLevelCoAPClient_ with low level functionality
* Block transfer is supported

## Supported frameworks

* .NET 9

## Nuget

This library is available as a nuget package: <https://www.nuget.org/packages/Matizk.CoAP/>

## Contributions

If you want to contribute to this project just create a pull request. But only pull requests which are matching the code style of this library will be accepted. Before creating a pull request please have a look at the library to get an overview of the required style.
Also additions and updates in the Wiki are welcome.

## License

MIT License

CoAPnet Copyright (c) 2016-2025 Christian Kratky & Mateusz Kramarz

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
