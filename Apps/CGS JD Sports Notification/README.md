# CGS Romania - JD Sports Queue Notification App

<img src="https://i.imgur.com/FymbEjs.png"/>

# Why this app?

I've decided to create this app because when we are on-call working from home I really hate to manually check the queue, so I simple thought to use my skills to create a simple app which will check the queue and send a notification when an open/new ticket it's found.

And to not forget thst it's also helping my colleagues 💪 

## Screenshot (v1.0 build 2080)

<p align="center"><img src="https://i.imgur.com/iomBPZE.jpg" width="50%" height="50%"/></p>


### Personal notes

I've learnt a lot during my journey to build this simple app.<br>

When I start, I thought that will not take me more than 1 week to finish it...<br>
I was reeeeally wrong!<br><br>
This app took me 1 month and ~1 week to finish it, mostly because I never written an app like this before (*the background working process made me to lose a lot of time*) and then because I had a lot of troubles to find a way to create a fast/reliable headless browser with JS support (Firstly I was using the Xamarin.Forms.WebView, which wasn't ok because does not work when the activity it's gone), then I discovered the Android WebView which was perfect, but sadly the JavaScript evaluation function isn't *async* and for the return output uses a callback instead of just returning the string like the Xamarin.Forms variant.

So I started building my own variant with a callback (I must admit that I used the code from [here](https://forums.xamarin.com/discussion/43759/how-do-i-await-the-result-of-evaluatejavascript)) and then I was like "Ok, the hard part it's gone, now the "easy" part, scrapping the website to get the data I need..."<br>
Wrooooong again! =)

Now the problem was the core functionality of this app, running in the background without consuming too much battery/RAM and of course firing the notification at the correct time selected by the user, I had a lot of problems with this, because I discovered too late that almost every OEM uses their own method to "optimize the battery" (*OEMs, please just stop doing this, **doze from AOSP it's enough!***).

So I lost a lot of days to understand why my app sometimes was working and sometimes not, I tried a lot of different methods (which this lead me to rewrite the code several times) until I understood exactly how this does work, how AOSP/OEMs tries to optimizes the battery and then I was able to create something which I'm satisfied of.

So, yeah, I had a lot of fun while building this app 😎


I think this meme it's perfect to resume my work on this app 😂

<p align="center"><img src="https://i.imgur.com/o6aaWjn.jpg" width="50%" height="50%"/></p>
