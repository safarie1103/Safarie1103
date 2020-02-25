library(data.table)
library(dplyr)
library(ggplot2)
library(stringr)
library(DT)
library(tidyr)
library(corrplot)
library(leaflet)
library(lubridate)
library(ggmap)
# VIM library for using 'aggr'
library(VIM)

properties <- read.csv("zillow-prize-1/properties_2016.csv")
transactions <- read.csv("zillow-prize-1/transactions_2016.csv")
summary(properties)
summary(transactions)

head(transactions)
head(properties)

mean(transactions$logerror)
median(transactions$logerror)
std <- sd(transactions$logerror)

names(properties)
names(transactions)

# Rename features

properties <- properties %>% rename(
  parcelid = parcelid,
  build_year = yearbuilt,
  area_basement = basementsqft,
  area_patio = yardbuildingsqft17,
  area_shed = yardbuildingsqft26, 
  area_pool = poolsizesum,  
  area_lot = lotsizesquarefeet, 
  area_garage = garagetotalsqft,
  area_firstfloor_finished = finishedfloor1squarefeet,
  area_total_calc = calculatedfinishedsquarefeet,
  area_base = finishedsquarefeet6,
  area_live_finished = finishedsquarefeet12,
  area_liveperi_finished = finishedsquarefeet13,
  area_total_finished = finishedsquarefeet15,  
  area_unknown = finishedsquarefeet50,
  num_unit = unitcnt, 
  num_story = numberofstories,  
  num_room = roomcnt,
  num_bathroom = bathroomcnt,
  num_bedroom = bedroomcnt,
  num_bathroom_calc = calculatedbathnbr,
  num_bath = fullbathcnt,  
  num_75_bath = threequarterbathnbr, 
  num_fireplace = fireplacecnt,
  num_pool = poolcnt,  
  num_garage = garagecarcnt,  
  region_county = regionidcounty,
  region_city = regionidcity,
  region_zip = regionidzip,
  region_neighbor = regionidneighborhood,  
  tax_total = taxvaluedollarcnt,
  tax_building = structuretaxvaluedollarcnt,
  tax_land = landtaxvaluedollarcnt,
  tax_property = taxamount,
  tax_year = assessmentyear,
  tax_delinquency = taxdelinquencyflag,
  tax_delinquency_year = taxdelinquencyyear,
  zoning_property = propertyzoningdesc,
  zoning_landuse = propertylandusetypeid,
  zoning_landuse_county = propertycountylandusecode,
  flag_fireplace = fireplaceflag, 
  flag_tub = hashottuborspa,
  quality = buildingqualitytypeid,
  framing = buildingclasstypeid,
  material = typeconstructiontypeid,
  deck = decktypeid,
  story = storytypeid,
  heating = heatingorsystemtypeid,
  aircon = airconditioningtypeid,
  architectural_style= architecturalstyletypeid
)
names(properties)


transactions <- transactions %>% rename(
  parcelid = parcelid,
  date = transactiondate
)

#Scatter plot of log error
transactions %>%
  ggplot(aes(x=date,y=logerror))  + ggtitle("loggerror") +
  geom_point()

# Histogram  of logerror 
transactions %>%
  ggplot(aes(x=logerror)) +  ggtitle("logerror") +
  geom_histogram()

# Histogram of logerror with binning
transactions %>% 
  ggplot(aes(x=logerror)) +  ggtitle("logerror binned") +
  geom_histogram(bins=400, fill="blue")+
  theme_bw()+theme(axis.title = element_text(size=16),axis.text = element_text(size=14))+
  ylab("Count")+coord_cartesian(x=c(0,0.5))


# Create year_month column and a column to show absolute logerror
transactions$year_month <- make_date(year=year(transactions$date),month=month(transactions$date)) 
transactions$abs_logerror <- abs(transactions$logerror)

head(transactions)

#Scatter plot of absolute log error
transactions %>%
  ggplot(aes(x=date,y=abs_logerror)) + ggtitle("abs_loggerror") +
  geom_point()

# Histogram  of abs_loggerror 
transactions %>%
  ggplot(aes(x=abs_logerror)) +  ggtitle("abs_loggerror") +
  geom_histogram()

# Histogram of abs_logerror with binning
transactions %>% 
  ggplot(aes(x=abs_logerror)) +  ggtitle("abs_loggerror binned") +
  geom_histogram(bins=400, fill="blue")+
  theme_bw()+theme(axis.title = element_text(size=16),axis.text = element_text(size=14))+
  ylab("Count")+coord_cartesian(x=c(0,0.5))

# graph of abs_logerror groupped by month of year 
transactions %>% 
  group_by(year_month) %>% summarize(mean_abs_logerror = mean(abs_logerror)) %>% 
  ggplot(aes(x=year_month,y=mean_abs_logerror)) + 
  geom_line(size=1.5, color="red")+ ggtitle("abs_loggerror by month") +
  geom_point(size=5, color="red")+theme_bw()


# graph of abs_logerror within strandard deviation
within_sd <- subset(transactions,abs_logerror <= std)

within_sd %>%
  group_by(year_month) %>% summarize(mean_abs_logerror = mean(abs_logerror)) %>% 
  ggplot(aes(x=year_month,y=mean_abs_logerror)) + 
  geom_line(size=1.5, color="red")+ggtitle("abs_loggerror with std") +
  geom_point(size=5, color="blue")+theme_bw()



# Cleanup features with too many missing values
missing_values <- properties %>% summarize_each(funs(sum(is.na(.))/n()))
missing_values
missing_values <- gather(missing_values, key="feature", value="missing_pct")
missing_values

# Plot the missing value histogram
missing_values %>% 
  ggplot(aes(x=reorder(feature,-missing_pct),y=missing_pct)) +
  geom_bar(stat="identity",fill="red")+
  coord_flip()+theme_bw()

# select features with less than 75% missing values
good_features <- filter(missing_values, missing_pct<0.75)


good_features


#Correlation of num_ features with logerror and abs_logerror:
#num_bathroom num_bedroom num_bathroom_calc num_bath num_garage num_room num_unit abs_logerror

num_features <- good_features$feature[str_detect(good_features$feature,'num_')]
properties_and_transacions <- transactions %>% left_join(properties, by="parcelid") 

# correlation with logerror
dataset_num_features <- properties_and_transacions %>% select(one_of(c(num_features,"logerror","abs_logerror")))
head(dataset_num_features)

sample = dataset_num_features[sample(nrow(dataset_num_features), 10000),]
#show correlation using ggpairs using a subset of data(~10%)
GGally::ggpairs(sample,use="complete.obs")

#show correlation using corrplot
corrplot(cor(dataset_num_features, use="complete.obs"),type="lower")


# Create a group of absolute log error into percentiles from worst to best

transactions <- transactions %>% 
  mutate(percentile = cut(abs_logerror,quantile(abs_logerror, probs=c(0, 0.1, 0.25, 0.75, 0.9, 1),names = FALSE),include.lowest = TRUE,labels=FALSE))
head(transactions)


perc_best <- transactions %>% 
  filter(percentile == 1) %>% 
  sample_n(5000) %>% 
  left_join(properties, by="parcelid")
perc_worst <- transactions %>% 
  filter(percentile == 5) %>% 
  sample_n(5000) %>% 
  left_join(properties, by="parcelid")
perc_typical<- transactions %>% 
  filter(percentile == 3) %>% 
  sample_n(5000) %>% 
  left_join(properties, by="parcelid")

perc_best <- perc_best %>% mutate(type="best_fit")
perc_worst <- perc_worst %>% mutate(type="worst_fit")
perc_typical <- perc_typical %>% mutate(type="typical_fit")

perc_all <- bind_rows(perc_worst,perc_typical,perc_best)
perc_all <- perc_all %>% mutate(type = factor(type,levels = c("worst_fit", "typical_fit", "best_fit")))

col_pal <- 3
# Density plot shows density of worst predicitons is lower in lower latitudes , but higher in  median lattitude and lowwer around and above 34400000. 
perc_all %>% ggplot(aes(x=latitude, fill=type, color=type)) + 
  geom_line(stat="density", size=1.2) + 
  theme_bw() + scale_fill_brewer(palette=col_pal)+scale_color_brewer(palette=col_pal)

properties_and_transacions %>% ggplot(aes(x=latitude,y=abs_logerror))+geom_smooth(color="red")+theme_bw()

# Examine denisity of longitude. Worst predictions are in the -118500000 and -118000000
perc_all %>% ggplot(aes(x=longitude, fill=type, color=type)) + 
  geom_line(stat="density", size=1.2) + 
  theme_bw() + scale_fill_brewer(palette=col_pal)+scale_color_brewer(palette=col_pal)

# PLot of abs_logerror against longitude
properties_and_transacions %>% ggplot(aes(x=longitude,y=abs_logerror))+geom_smooth(color="red")+theme_bw()


# area_total_finished

# Examine denisity of longitude. Worst predictions are in the -118500000 and -118000000
perc_all %>% ggplot(aes(x=area_total_finished, fill=type, color=type)) + 
  geom_line(stat="density", size=1.2) + 
  theme_bw() + scale_fill_brewer(palette=col_pal)+scale_color_brewer(palette=col_pal)

# PLot of abs_logerror against longitude
properties_and_transacions %>% ggplot(aes(x=area_total_finished,y=abs_logerror))+geom_smooth(color="red")+theme_bw()

# num_room

# Examine denisity of longitude. Worst predictions are in the -118500000 and -118000000
perc_all %>% ggplot(aes(x=num_room, fill=type, color=type)) + 
  geom_line(stat="density", size=1.2) + 
  theme_bw() + scale_fill_brewer(palette=col_pal)+scale_color_brewer(palette=col_pal)

# PLot of abs_logerror against longitude
properties_and_transacions %>% ggplot(aes(x=num_room,y=abs_logerror))+geom_smooth(color="red")+theme_bw()

lat <- range(properties$latitude/1e06,na.rm=T)
lon <- range(properties$longitude/1e06,na.rm=T)

tmp <- properties %>% 
  sample_n(10000) %>% 
  select(parcelid,longitude,latitude) %>% 
  mutate(lon=longitude/1e6,lat=latitude/1e6) %>% 
  select(parcelid,lat,lon) %>% 
  left_join(transactions,by="parcelid")

API_key <- "AIzaSyDBRpHoEPCnVtcFPit-jVx26fkbrAemzN0"
register_google(key = API_key)
register_google(key = API_key, write = TRUE)


# map of sample number of properties
Map <- get_map("Log Angels", zoom = 10)
ggmap(Map) + geom_point(aes(x = lon[], y = lat[]),data = tmp  ) +
  ggtitle("properties in LA area")

Map <- get_map("Orange County", zoom = 10)
ggmap(Map) + geom_point(aes(x = lon[], y = lat[]),data = tmp  ) +
  ggtitle("properties in Orange county area")


Map <- get_map("Ventura County", zoom = 10)
ggmap(Map) + geom_point(aes(x = lon[], y = lat[]),data = tmp  ) +
  ggtitle("properties in Ventura county area")

sample_transactions <- transactions %>% 
  sample_n(10000) %>% 
  left_join(properties,by="parcelid") %>% 
  select(parcelid,longitude,latitude, abs_logerror) %>% 
  mutate(lon=longitude/1e6,lat=latitude/1e6) %>% 
  select(parcelid,lat,lon, abs_logerror)

# Color quantile spreads the abs_logerror into quntiles and associated share of yellow to red

qpal <- colorQuantile("YlOrRd", sample_transactions$abs_logerror, n = 7)


leaflet(sample_transactions) %>% 
  addTiles() %>% 
  fitBounds(lon[1],lat[1],lon[2],lat[2]) %>% 
  addCircleMarkers(stroke=FALSE, color=~qpal(abs_logerror),fillOpacity = 1) %>% 
  addLegend("bottomright", pal = qpal, values = ~abs_logerror,title = "Absolute logerror",opacity = 1) %>% 
  addMiniMap()

