# Load the DBI package

library("DBI")
# Edit dbConnect() call
con <- dbConnect(RMySQL::MySQL(), 
                 dbname = "tweater", 
                 host = "courses.csrrinzqubik.us-east-1.rds.amazonaws.com", 
                 port = 3306,
                 user = "student",
                 password = "datacamp")
dbListTables(con)
users = dbReadTable(con,"users")
users
tables = c(dbListTables(con))
tables
# Import all tables
table_names <- dbListTables(con)
tables <- lapply(table_names, dbReadTable, conn = con)

# Print out tables
tables
# Display structure of tables
str(tables)

elisabeth = dbGetQuery(con, "SELECT tweat_id FROM comments  WHERE user_id = 1")
# Print elisabeth
elisabeth

latest <- dbGetQuery(con, "SELECT post FROM tweats  WHERE date > '2015-09-21'")

# Print latest
latest

specific <-  dbGetQuery(con, "SELECT message from comments where tweat_id = 77 AND user_id > 4")
# Print specific
specific

# Create data frame short
short <-  dbGetQuery(con, "SELECT id, name from users where CHAR_LENGTH(name) < 5")


# Print short

short
join1 <- dbGetQuery(con, "SELECT name, post FROM users INNER JOIN tweats on 
                  users.id = user_id WHERE date > '2015-09-19'")

join1
join2 <- dbGetQuery(con ,"SELECT post, message
  FROM tweats INNER JOIN comments on tweats.id = tweat_id
    WHERE tweat_id = 77")
join2


res <- dbSendQuery(con ,"SELECT post, message
  FROM tweats INNER JOIN comments on tweats.id = tweat_id
                    WHERE tweat_id = 77")
dbFetch(res,n=2)
dbFetch(res)
dbClearResult(res)

dbFetch(res)

long_tweats  <- dbGetQuery(con, "SELECT post,date  FROM tweats where CHAR_LENGTH(post) > 40 ")
# Print long_tweats
print(long_tweats)
dbDisconnect(con)


# Load the readr package
library(readr)

# Import the csv file: pools
url_csv <- "http://s3.amazonaws.com/assets.datacamp.com/production/course_1478/datasets/swimming_pools.csv"

pools <- read.csv(url_csv)
# Import the txt file: potatoes
url_delim <- "http://s3.amazonaws.com/assets.datacamp.com/production/course_1478/datasets/potatoes.txt"
potatoes <- read.delim(url_delim)

# Print pools and potatoes
pools
potatoes

# Load the readr package
library(readr)

# Import the csv file: pools
url_csv <- "http://s3.amazonaws.com/assets.datacamp.com/production/course_1478/datasets/swimming_pools.csv"

pools <- read_csv(url_csv)
# Import the txt file: potatoes
url_delim <- "http://s3.amazonaws.com/assets.datacamp.com/production/course_1478/datasets/potatoes.txt"
potatoes <- read_tsv(url_delim)

# Print pools and potatoes
pools
potatoes

# https URL to the swimming_pools csv file.
url_csv <- "https://s3.amazonaws.com/assets.datacamp.com/production/course_1478/datasets/swimming_pools.csv"

# Import the file using read.csv(): pools1

pools1 <- read.csv(url_csv)
# Load the readr package
library(readr)

# Import the file using read_csv(): pools2

pools2 <- read_csv(url_csv)
# Print the structure of pools1 and pools2
str(pools1)
str(pools2)



# Load the readxl and gdata package
library(readxl)
library(gdata)

# Specification of url: url_xls
url_xls <- "http://s3.amazonaws.com/assets.datacamp.com/production/course_1478/datasets/latitude.xls"

# Import the .xls file with gdata: excel_gdata

excel_gdata <- read.xls(url_xls,perl = "C:/Strawberry/perl/bin/perl.exe")
excel_gdata
# Download file behind URL, name it local_latitude.xls
###download.file(url_xls,"local_latitude.xls")

# Import the local .xls file with readxl: excel_readxl
#excel_readxl <- read_excel("local_latitude.xls")


###str(excel_readxl)

# https URL to the wine RData file.
url_rdata <- "https://s3.amazonaws.com/assets.datacamp.com/production/course_1478/datasets/wine.RData"

# Download the wine file to your working directory

download.file(url_rdata,"wine_local.RData")
# Load the wine data into your workspace using load()

load("wine_local.RData")
# Print out the summary of the wine data
summary(wine)
wine
# Load the httr package
library(httr)

# Get the url, save response to resp
url <- "http://www.example.com/"

resp <- GET(url)
# Print resp

resp
# Get the raw content of resp: raw_content
raw_content <- content(resp,as = "raw")

# Print the head of raw_content
head(raw_content)


# httr is already loaded

# Get the url
url <- "http://www.omdbapi.com/?apikey=72bc447a&t=Annie+Hall&y=&plot=short&r=json"

resp <- GET(url)
# Print resp
resp

# Print content of resp as text

content(resp,as="text")
# Print content of resp
content(resp)
