## Importing packages
# library(tidyverse) # metapackage with lots of helpful functions
library(dplyr)
library(plotly)
# library("fpc")
# library("factoextra")
library(cluster)
# VIM library for using 'aggr'
library(VIM)
library(lubridate)
library(ggmap)
library(DT)
library(dplyr)


apr14 <- read.csv("uberdataset/uber-raw-data-apr14.csv")
may14 <- read.csv("uberdataset/uber-raw-data-may14.csv")
june14 <- read.csv("uberdataset/uber-raw-data-jun14.csv")
july14 <- read.csv("uberdataset/uber-raw-data-jul14.csv")
aug14 <- read.csv("uberdataset/uber-raw-data-aug14.csv")
sep14 <- read.csv("uberdataset/uber-raw-data-sep14.csv")

apr14 <- apr14[sample(nrow(apr14), 10000),]
may14 <- may14[sample(nrow(may14), 10000), ]
june14 <- june14[sample(nrow(june14), 10000), ]
july14 <- july14[sample(nrow(july14), 10000), ]
aug14 <- aug14[sample(nrow(aug14), 10000), ]
sep14 <- sep14[sample(nrow(sep14), 10000), ]


data14  <- bind_rows(apr14,may14,june14,july14,aug14,sep14)
nrow(data14)
summary(data14 )


head(data14)

# 'aggr' plots the amount of missing/imputed values in each column
aggr(data14)


# Separate or mutate the Date/Time columns
data14$Date.Time <- mdy_hms(data14$Date.Time)
data14$Year <- factor(year(data14$Date.Time))
data14$Month <- factor(month(data14$Date.Time))
data14$Day <- factor(day(data14$Date.Time))
data14$Weekday <- factor(wday(data14$Date.Time))
data14$Hour <- factor(hour(data14$Date.Time))
data14$Minute <- factor(minute(data14$Date.Time))
data14$Second <- factor(second(data14$Date.Time))


data14$Month


head(data14,n=10)

set.seed(20)
clusters <- kmeans(data14[,2:3], 5)

# Save the cluster number in the dataset as column 'Borough'
data14$Borough <- as.factor(clusters$cluster)
head(data14,n=10)
str(clusters)

API_key <- "AIzaSyDBRpHoEPCnVtcFPit-jVx26fkbrAemzN0"
register_google(key = API_key)
register_google(key = API_key, write = TRUE)

NYCMap <- get_map("New York", zoom = 10)
ggmap(NYCMap) + geom_point(aes(x = Lon[], y = Lat[], colour = as.factor(Borough)),data = data14) +
  ggtitle("NYC Boroughs using KMean")




data14$Month <- as.double(data14$Month)
head(data14)
month_borough_14 <- count_(data14, vars = c('Month', 'Borough'), sort = TRUE) %>% 
  arrange(Month, Borough)
datatable(month_borough_14)

monthly_growth <- month_borough_14 %>%
  mutate(Date = paste("04", Month)) %>%
  ggplot(aes(Month, n, colour = Borough)) + geom_line() +
  ggtitle("Uber Monthly Growth - 2014")
monthly_growth
