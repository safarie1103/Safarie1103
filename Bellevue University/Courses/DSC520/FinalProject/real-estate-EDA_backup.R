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

properties_2016 <- read.csv("zillow-prize-1/properties_2016.csv")
properties_2017 <- read.csv("zillow-prize-1/properties_2017.csv")
transactions_2016 <- read.csv("zillow-prize-1/transactions_2016.csv")
transactions_2017 <- read.csv("zillow-prize-1/transactions_2017.csv")
transactions <- rbind(transactions_2016,transactions_2017)
properties <- rbind(properties_2016,properties_2017)


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

properties_2016 <- properties_2016 %>% rename(
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
names(properties_2016)

properties_2017 <- properties_2017 %>% rename(
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
names(properties_2017)

transactions <- transactions %>% rename(
  parcelid = parcelid,
  date = transactiondate
)
transactions_2016 <- transactions_2016 %>% rename(
  parcelid = parcelid,
  date = transactiondate
)
names(transactions_2016)
transactions_2017 <- transactions_2017 %>% rename(
  parcelid = parcelid,
  date = transactiondate
)
names(transactions_2017)

#Scatter plot of log error
transactions_2016 %>%
  ggplot(aes(x=date,y=logerror)) +
  geom_point()

transactions_2016$year_month <- make_date(year=year(transactions_2016$date),month=month(transactions_2016$date)) 
transactions_2016$abs_logerror <- abs(transactions_2016$logerror)

head(transactions_2016)

transactions_2016 %>%
  ggplot(aes(x=date,y=abs_logerror)) +
  geom_point()

# Scatter plot of abs_logerror for 2016 transactions
transactions_2016 %>%
  ggplot(aes(x=abs_logerror)) +
  geom_histogram()

# Histogram of abs_logerror for 2016 transactions
transactions_2016 %>% 
  ggplot(aes(x=abs_logerror)) + 
  geom_histogram(bins=400, fill="blue")+
  theme_bw()+theme(axis.title = element_text(size=16),axis.text = element_text(size=14))+
  ylab("Count")+coord_cartesian(x=c(0,0.5))

# graph of abs_logerror groupped by month of year for 2016 transactions
transactions_2016 %>% 
  group_by(year_month) %>% summarize(mean_abs_logerror = mean(abs_logerror)) %>% 
  ggplot(aes(x=year_month,y=mean_abs_logerror)) + 
  geom_line(size=1.5, color="red")+
  geom_point(size=5, color="red")+theme_bw()

transactions_2017$year_month <- make_date(year=year(transactions_2017$date),month=month(transactions_2017$date)) 
transactions_2017$abs_logerror <- abs(transactions_2017$logerror)


head(transactions_2017)
# graph of abs_logerror groupped by month of year for 2017 transactions
transactions_2017 %>% 
  group_by(year_month) %>% summarize(mean_abs_logerror = mean(abs_logerror)) %>% 
  ggplot(aes(x=year_month,y=mean_abs_logerror)) + 
  geom_line(size=1.5, color="blue")+
  geom_point(size=5, color="orange")+theme_bw()


within_sd <- subset(transactions,logerror <= std || logerror >= -std )

within_sd %>%
  ggplot(aes(x=date,y=logerror)) +
  geom_point()


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

good_features <- filter(missing_values, missing_pct<0.75)


good_features
#Correlation of the following variables with absolute logerror
#num_ features:
#num_bathroom num_bedroom num_bathroom_calc num_bath num_garage num_room num_unit abs_logerror

num_features <- good_features$feature[str_detect(good_features$feature,'num_')]
num_features

# Create a dataset of the num_features
properties_and_transacions_2016 <- transactions_2016 %>% left_join(properties_2016, by="parcelid") 
dataset_num_features <- properties_and_transacions_2016 %>% select(one_of(c(vars,"abs_logerror")))
head(properties_and_transacions_2016)
GGally::ggpairs(dataset_num_features,use="complete.obs")

corrplot(cor(dataset_num_features, use="complete.obs"),type="lower")


clusters <- kmeans(properties_and_transacions_2016[,c('longitude','latitude')], 5)
clusters$cluster
# Save the cluster number in the dataset as column 'Borough'
properties_and_transacions_2016$Borough <- as.factor(clusters$cluster)
head(properties_and_transacions_2016,n=10)
str(clusters)

API_key <- "AIzaSyDBRpHoEPCnVtcFPit-jVx26fkbrAemzN0"
register_google(key = API_key)
register_google(key = API_key, write = TRUE)


sample1 = properties_and_transacions_2016[sample(nrow(properties_and_transacions_2016), 10000),]
LAMap <- get_map("Log Angels", zoom = 10)
ggmap(LAMap) + geom_point(aes(x = longitude[], y = latitude[], colour = as.factor(Borough)),data = sample1  ) +
  ggtitle("Log Angels Boroughs using KMean")



tmp <- cor_tmp %>% select(one_of(c(vars,"abs_logerror")))

corrplot(cor(tmp, use="complete.obs"),type="lower")
