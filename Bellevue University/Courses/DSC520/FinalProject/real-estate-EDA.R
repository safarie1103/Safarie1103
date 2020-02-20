library(data.table)
library(dplyr)
library(ggplot2)
library(stringr)
library(DT)
library(tidyr)
library(corrplot)
library(leaflet)
library(lubridate)

dataset <- read.csv("real-estate-price-prediction/Real estate.csv")
transactions <- read.csv("zillow-prize-1/train_2016.csv")
subset(transactions,logerror < 0)
max(transactions$logerror)
min(transactions$logerror)

mean(transactions$logerror)
median(transactions$logerror)
std(transactions$logerror)
names(properties)
names(transactions)

#Scatter plot of log error
transactions %>%
  ggplot(aes(x=date,y=logerror)) +
  geom_point()

missing_values <- dataset %>% summarize_each(funs(sum(is.na(.))/n()))
missing_values
missing_values <- gather(missing_values, key="feature", value="missing_pct")
missing_values
missing_values %>% 
  ggplot(aes(x=reorder(feature,-missing_pct),y=missing_pct)) +
  geom_bar(stat="identity",fill="red")+
  coord_flip()+theme_bw()
