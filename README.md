# heicConverter
Its main goal is to easly convert [.heic](https://en.wikipedia.org/wiki/High_Efficiency_Image_File_Format) images created by iPhones to other file types, like jpg or png.  
It is utilizing the [HEIF Image Extensions](https://www.microsoft.com/en-us/p/heif-image-extensions/9pmmsr1cgpwg?activetab=pivot:overviewtab) by Microsoft. You need to have it installed, otherwise it will not work.  
You can install via the Setup.msi (built by the [WiX](https://wixtoolset.org/documentation/manual/v3/) project).  
  
![Capture](https://user-images.githubusercontent.com/11587908/135081489-5b521eab-f801-4e34-b36b-628e16259d32.PNG)  
  
It will install the program to the program files folder and register some sub menu commands (Convert to -> jpg, png, etc...) for windows explorer in the registry.  
  
![Untitled](https://user-images.githubusercontent.com/11587908/135080944-ae2007df-15b2-4503-b75f-e54bd88babb2.jpg)  
  
The Convert to sub menu will only appear for .heic files.
