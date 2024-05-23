# ProjectWebScraping

This is program for scraping data from the site https://books.toscrape.com/ and save them locally on your computer.
Since I'm new to web scraping i decided to use HtmlAgilityPack for DOM traversing. I used 'async' and 'await' so that the getting of the pages to scrape and the scraping can happen simultaneously and speed up the appilcation.
The methods can be considered a bit bloated but i still wanted to keep them as such, because I think it's easier to follow, especially the 'HandleBook'-method. 

# How to run
Download the program as a Zip-file or clone it using your editor of choice. Open the Command prompt and navigate to the Executable folder of the project. Then type ProjectWebScraping.exe and the program will run. It will show when the scraping for each category starts, when it's finished and how many books are found in each category.
