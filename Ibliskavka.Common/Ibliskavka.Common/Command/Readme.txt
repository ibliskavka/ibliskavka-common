This is a simple implementation of command design pattern.

The idea is that each command has an undo method.
If 1 command fails during execution, the work already completed can be un-done.

This is useful if you need an atomic operation and you dont have a transaction scope. Example: writing records into multiple databases or webservices.

Ofcourse the undo operation can fail too, but you have to start somewhere.