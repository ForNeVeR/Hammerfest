Hammerfest [![Status Zero][status-zero]][andivionian-status-classifier]
==========

Hammerfest is a project to implement a lobby server for [Command & Conquer 3: Kane's Wrath][cnc3].

Implementation Status
---------------------

Wew are on the stage when the game is able to show the login screen.

Overall, we are still more at the exploration stage; scope of work is to be determined.

Visit [the issue tracker][issues] if you want to know more.

Usage
-----

Currently Hammerfest is only suitable for development mode, and only on Windows.

**⚠ For now, the administrator privileges are required to run the server, since it modifies the system `hosts` file.**

To start the server:

```console
# dotnet run --project Hammerfest.Server
```

It reads the configuration from the `appsettings.json` file.

The server performs the following operations:
- Temporarily overrides the contents of `%SystemRoot%\System32\drivers\etc\hosts` file, adding the server host to it.

  Pass `false` in the `Dns.Enable` configuration file if this is unwanted.
- Starts a ServServ HTTP implementation on port `80`.

Press `Ctrl+C` to terminate the server.

Legal Disclaimers
-----------------

_Thanks to _

- This project is not affiliated with or endorsed by EA in any way. Command & Conquer is a trademark of Electronic Arts.
- This project is non-commercial. The source code is available for free and always will be.
- This is a blackbox re-implementation project. The code in this project was written based on reading data files, and observing the game(s) running. In some cases the code was written based on specs available on the Internet.
  
  I believe this puts the project in the clear, legally speaking. If someone disagrees, please reach the lead maintainer, [Friedrich von Never][fornever].

- No assets from the original games are included in this repo.

Documentation
-------------

- [License (MIT)][docs.license]
- [Code of Conduct (adapted from the Contributor Covenant)][docs.code-of-conduct]

Acknowledgments
---------------

- Thanks to the [OpenSAGE][open-sage] project for some ideas on the wording in the **Legal Disclaimers** section.
- Thanks to the [GenServer][gen-server] project for documenting their ServServ implementation.

[andivionian-status-classifier]: https://github.com/ForNeVeR/andivionian-status-classifier#status-zero-
[cnc3]: https://cnc.fandom.com/wiki/Command_%26_Conquer_3:_Kane%27s_Wrath
[docs.code-of-conduct]: CODE_OF_CONDUCT.md
[docs.license]: LICENSE.md
[fornever]: https://github.com/ForNeVeR/
[gen-server]: https://github.com/SySAttic/GenServer
[issues]: https://github.com/ForNeVeR/Hammerfest/issues
[open-sage]: https://github.com/OpenSAGE/OpenSAGE
[status-zero]: https://img.shields.io/badge/status-zero-lightgrey.svg
