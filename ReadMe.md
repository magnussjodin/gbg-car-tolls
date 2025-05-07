# Project Overview

This document explains how to execute the application and provides an overview of the steps I followed to build it.
The resulting app should be seen as a prototype of an idea, as it does not persist any registered passages and only works properly for dates in 2013.
(Added 2025-05-07) I came to think of, that the app is actually mostly treating success scenarios in a good way. And even if the error handling is found in the framework, I think some of it might be silently swallowed by this app, especially in the registration part. The app could though be used for showcasing the business concept.

## How to Execute the Application

1. Ensure you have [required software/tools] installed (C# etc).
2. Open a terminal in the project directory.
3. Execute "cd source"
4. To start the application, execute "dotnet run --project VehiclePassageTolls". In the app you will see a basic number of registered passages and the fee they result in, but also register new passages.
5. To run the 45 unit tests, execute "dotnet test". There is still one warning, about a missing unit test in the TooFeeCalculator project, that could be ignored.

## How I Worked on the Task

1. **Understanding the task**:
   - Out of the task description I started by figuring out the needs and what value (app methods) the app must contain to fulfil the needs.
   - This was done in a Mural board, see provided file "Code test - Mural board.pdf"
   - I prioritized the must-have product values (darker) for the most needed needs
   - Now, I started to look at the code and came up with needed tests for the existing methods in the TollCalculator
   - I also detected a number of bugs or other improvements I wanted to do
   - (The PassageTollHandler tests and "Bugs and improvements in app" came later, when I had started the coding)

2. **Setting Up the Environment**:
   - Configured the development environment in Visual Studio Code.
   - Installed necessary dependencies and tools, this took some time as I have never developed on this computer.
   - To set up the project and dependencies, I took help of the chat bot.

3. **Development Process**:
   - **In general**:
   - Followed an iterative approach and implemented the needed tests step by step (small-ish commits), thereby sometimes confirming the bugs that I knew about, or detected new ones. The bugs are fixed in the same commit with a "Corr..." text.
   - Some of the commits are small or larger improvements.
   - In the digital whiteboard, I constantly tagged any finished parts with a hook or thumb.
   - I've tried to amend parts I found in doing the next thing to the right commit, but not more advanced shuffling like squashing
   - I have also tried to cover most parts with good unit tests, but one of the bugs I detected in visual testing at the end, where also the tests themselves were buggy. :)
   -
   - **Secured the provided TollCalculator framework code**:
   - After a day of coding and fighting with my slow computer, that I needed to restart a few times, I finished the work on the initial methods and classes. 
   - Two major bugs found can be mentioned: A problem in one of the timely definitions, not covering e g 9am. The mistake to think that DateTime.MilliSeconds gives out the timestamp in milliseconds.
   -
   - **Added missing pieces in the framework**:
   - I now started to develop the missing pieces: A method to get out Daily fees from different dates, and everything about registering and listing out passages and the daily fees out of those passages. All out from the prioritized needs I had in mind. In a real-case scenario, this is ofc done in a more iterative way, to constantly push out new things.
   - After yet another half-a-day, I was finished with the final number of tests and functionality in the TollFeeCalculator project.
   - One trap I went into in this phase, was to try to make Vehicle a key of the Dictionary. Reason is I wanted to make it possible to track if the same regnr was used for another vehicle type. But landed in the need for a EqualityComparer etc. In the end, the license nr - which should be unique - shown to be the best choice as key for the dictionary.
   -
   - **Building the visual app**:
   - I used the AI quite a lot in the final phase, where I got a lot of help to create the WPF app and XAML definitions and changes. It was fun to learn this new way of working closely to the AI companion!

4. **Development Decisions**:
   - I did not want to send the whole Vehicle object everywhere in the TollCalculator, but extracted just VehicleType, as that was the only thing the calculator should know about
   - I also did not want to encapsulate the Toll free knowledge in the Vehicle class. In earlier days, the classes were made rich of knowledge about themselves but imo it is more appropriate to tie the knowledge to where it is used
   - I have tried to use Linq expressions as much as possible, to both make the code more dense but also use the optimized performance in the .NET data collection framework
   - The calculation of toll free days should be replaced by looking up the dates using some standardized functionality in .NET, or a third-party library/service. Or the dates could possibly be stored in a database as static data. To have it as a calculation is very error prone.
   - I chose to do a visual on-prem app. Not a console app with its poorer experience, and where the input/output can be tricky to do well. And WPF vs WinForms - I like the declarativeness of WPF, and WinForms is quite outdated.
   - And ofc the registrations should be persisted in a database, but I draw the line there, to just let this app be a prototype.

5. **Future**:
   - A Vehicle Toll Fee managing product should definitely be a SaaS system, with this application as a web app
   - The passage registrations must be stored in a database
   - The app should provide good reporting & analysis parts, or a powerful export possibility
   - A Rest API ensure cameras can send registrations
   - An API also makes mobile apps possible and maybe third-party vendors can also build other products on top, like reporting
