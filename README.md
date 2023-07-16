# upgraded-tribble
A newer version of EnvelopeBudget as a web site

## Why are you making this project?
1. I do envelope budgeting on a console app but it would be nice to have a Web UI that also works on mobile and can be hosted in a LAN
2. Do things differently than what I do at work
3. Chance to learn to implement things such as 2FA into something I use

## Technical Decisions

### Maria DB/MySQL
An easy to use database that I can get up and running on ARM64 Linux.

### F#
Started with C# on this project, but decided to try something new. Other stuff I already built will use C# for now.

### HTMX
Trying out a world where nice, modern looking webpages doesn't need a whole lot of custom JavaScript.
Also I don't know webpack and I don't want to learn webpack.

### Picnic.css
I wanted something simple and smaller than bootstrap that still has all the features I could use.

## AdminPasswordRecovery
A console app to allow a local admin to reset a password.
The console app expects two arguments: the path to an ini file, and a user "email"


## Ini File Configuration
Ini is used because its easy to just have an ini file lying around that everything has a parser for and
can use in python projects that use odbc on linux.
```
[MariaEnvelope]
Servername= database_address
Database=envelope_database_name
UID=sql_username
PWD=sql_password
```
The EnvelopeDbUp app.settings has dbINISection field in case you want the envelope app to use a 
different sql user than what the website would use.
