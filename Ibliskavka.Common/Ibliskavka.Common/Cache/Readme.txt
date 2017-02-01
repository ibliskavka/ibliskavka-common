This is a simple implementation of an object cache. The objects are cached on the file system and memory and refreshed from the source when the file is deleted.
This can cache any data source, simply implement LoadFromSource and pick a unique file name.
Implement Initialize() if you need to set up other storage structures like dictionary, etc.
I use this for caching SharePoint CMS type data on the webserver.