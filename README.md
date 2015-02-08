# Nancy.AspNet.WebSockets

WebSocket support for Nancy (http://nancyfx.org/) applications hosted with ASP.NET.

## Dependencies

* Nancy.Hosting.Aspnet version 0.20.0
* Microsoft.WebSockets version 0.2.3
* .NET 4.5 (due to use of await/async)

## Installing

Install via the Package Manager Console in Visual Studio:

    Install-Package Nancy.AspNet.WebSockets

And for the test harness:

    Install-Package Nancy.AspNet.WebSockets.Testing

Or if you're more of a UI person, go to Tools &gt; NuGet Package Manager &gt; Manage NuGet Packages for Solution...,
and search for the package(s) there.

Nuget.org links:

* Main package: https://www.nuget.org/packages/Nancy.AspNet.WebSockets
* Test harness: https://www.nuget.org/packages/Nancy.AspNet.WebSockets.Testing

## Using

Please see the wiki for full usage instructions.

## Building/contributing

Feel free to contribute bug fixes and improvements.

To build outside of Visual Studio, you must have npm and grunt installed. To install
the necessary dependencies, run:

    npm install .
    npm install -g grunt-cli

Simply run `grunt` without arguments to compile and run all tests.

## Author/contact

Per Roveg&aring;rd

Follow me on Twitter: @provegard

Also read my blog: http://programmaticallyspeaking.com

## License

Copyright 2015 Per Roveg&aring;rd

All code is licensed under the MIT license. See the LICENSE file for more details.


