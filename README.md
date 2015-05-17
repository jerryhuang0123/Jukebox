# Jukebox
Developed for Windows 10 in C#. 

Jukebox player created during Hackathon with 3 other participants in a 24 hour span. 
Contains a server and a client side, where the client side can search and vote for a song they desire. 
The playlist currently on will also be visible to the client. 
Each client are identified by a GUID, generated at startup once the client's name is entered.
The server, or the DJ, can upload the music library using File crawler, and based on the votes decide the order of the playlist.
The music databases are hosted on Microsoft Azure. 
