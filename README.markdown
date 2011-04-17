       _____  _       _      _       
      / ____|| |     (_)    | |      
     | |     | |      _   __| |  ___ 
     | |     | |     | | / _` | / _ \
     | |____ | |____ | || (_| ||  __/
      \_____||______||_| \__,_| \___|           
    
       CLIDE is a CLI IDE for .NET
    
    Run clide help for help documentation

Install
-------

    Install-Package cli-ide

or

[Download the .exe](https://github.com/remi/clide/raw/releases/bin/Release/clide.exe)

Screencast
----------

You can watch the introductory screencast at: [http://remi.org/2011/04/17/clide](http://remi.org/2011/04/17/clide)

As I add more features, I might make more screencasts and document more of the features. 
It really depends on whether or not other people use CLIDE.  Right now, it's just a tool that I find useful!

Usage
-----

Running `clide help` gives you a usage overview:

    CLIDE is a CLI IDE for .NET
    
      Usage:
        clide -h/--help
        clide -v/--version
        clide command [arguments...] [options...]
    
      Examples:
        clide new ProjectName
        clide prop RootNamespace=Foo
        clide ref add ../lib/Foo.dll
        clide gen
    
      Further help:
        clide commands         list all 'clide' commands
        clide help <COMMAND>   show help on COMMAND
    
      Further information:
        https://github.com/remi/clide

Running `clide commands` lists the available commands:

    clide commands:
    
        commands      List the available commands
        help          Provide help on the 'clide' command
        content       Manage a Project's code content files
        generate      Generate a clide template
        info          Prints out info about environment/configuration
        new           Create a new project
        properties    Get or set configuration properties
        references    Manage a Project's references
        sln           Alias for 'solution'
        solution      Create/Edit solution files
        source        Manage a Project's code source files

Running `clide help [some command]` will print out the help documentation for that command:

    Usage: clide new [ProjectName] [options]
    
      If the ProjectName isn't specified, the folder name of the current directory is used
    
      Options:
        -b, --bare       Creates a bare csproj with just a <Project> node
        -e, --exe        Sets project OutputType to exe
        -w, --winexe     Sets project OutputType to winexe
        -l, --library    Sets project OutputType to library
        -s, --source     Define source files (same as clide source add)
        -c, --content    Define content files (same as clide content add)
        -r, --reference  Define references (same as clide ref add)
    
      Common Options:
        -V, --verbose                      Can be set to true or a level, eg. INFO or WARN
        -D, --debug                        If set to true, additional debug data may be available
        -C, --config CONFIG                The project Configuration that you want to use
        -G, --global                       If set to true, this change is applied to all configurations
        -P, --project PROJECT              Name of project in solution of path to project file (csproj)
        -S, --solution SOLUTION            Path to the .sln solution file
        -F, --force                        Some options support --force to override warnings, etc
        -H, --help                         If set to true, we want to print out help/usage documentation
        -W, --working-dir WORKING_DIR      Sets the working directory. Defaults to the current directory
        -T, --templates CLIDE_TEMPLATES    The PATH used to find directories of clide templates
