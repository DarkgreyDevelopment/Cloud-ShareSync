# RandomFileGenerator
This is a very simple console app that generates random data files of various sizes for testing.

# Example Use
The application uses commandline arguments to control its operation.  
To specify the output directory pass the path to a valid directory in as a commandline argument.  
All subsequent files with be output to that directory until another directory is passed in.  
Otherwise files will be output into the applications root directory by default.  
All numbers will be interpreted and converted into megabytes.  


```batch
RandomFileGenerator.exe "C:\Path\To\Output\Directory" ".fileExtension" 1024 "1024mb" "1 gb" "C:\Path\To\Output\Directory2" ".fileExtension2" "1gb" "5.25gb" ".05tb"
```
