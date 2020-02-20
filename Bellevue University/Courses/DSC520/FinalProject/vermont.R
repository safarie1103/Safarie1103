library(data.table)
library(dplyr)
library(ggplot2)
library(stringr)
library(DT)
library(tidyr)
library(corrplot)
library(leaflet)
library(lubridate)

train <- read.csv("vt-nh-real-estate/train.csv")
test <- read.csv("vt-nh-real-estate/test.csv")
validate <- read.csv("vt-nh-real-estate/validate.csv")
head(train)

missing_values <- train %>% summarize_each(funs(sum(is.na(.))/n()))
missing_values
missing_values <- gather(missing_values, key="feature", value="missing_pct")
missing_values
missing_values %>% 
  ggplot(aes(x=reorder(feature,-missing_pct),y=missing_pct)) +
  geom_bar(stat="identity",fill="red")+
  coord_flip()+theme_bw()

missing_values
good_features <- filter(missing_values, missing_pct<0.75)
good_features
train$year_built <- as.Date(paste(train$year_built, 1, 1, sep = "-"))
train$year_built

tmp <- train %>% mutate(year_month = make_date(year=year(year_built),month=month(year_built)))
tmp %>% 
  group_by(year_built) %>% count() %>% 
  ggplot(aes(x=year_built,y=n)) +
  geom_bar(stat="identity", fill="red")
