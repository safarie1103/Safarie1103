
library(ggplot2)
library(stats)
library(GGally)
library(scatterplot3d)
library("readxl")

# From chapter 19.2 R for everyone
getwd()
setwd(".\\")
getwd()
housing1 <- read_excel("week-7-housing.xlsx")
#summary(Housing)
str(housing1)
head(housing1)
names(housing1)
#Cleanup Na's
columns_to_Cleanup <- c('Sale_Price','sale_warning','zip5','ctyname','building_grade','square_feet_total_living','bedrooms','bath_full_count','bath_half_count','bath_3qtr_count','year_built','year_renovated','current_zoning','sq_ft_lot','prop_type','present_use')
#complete.cases(housing1[,columns_to_Cleanup])
#which(complete.cases(housing1[,columns_to_Cleanup]))
#which(!complete.cases(housing1[,columns_to_Cleanup]))
housing <- housing1[complete.cases(housing1[,columns_to_Cleanup]) == TRUE, ]
head(housing)
housing$ValuePerSqFtLiving <- housing$Sale_Price/housing$square_feet_total_living
housing$ValuePerSqFtTotal <- housing$Sale_Price/housing$sq_ft_lot
names(housing)
str(housing)
mean(housing$ValuePerSqFtLiving)
median(housing$ValuePerSqFtLiving)
mean(housing$Sale_Price)
median(housing$Sale_Price)
max(housing$Sale_Price)
min(housing$Sale_Price)
mode(housing$Sale_Price)

ggplot(housing, aes(x=ValuePerSqFtLiving)) +
      geom_histogram(binwidth=10) + labs(x="Value per Square Foot Living")

ggplot(housing, aes(x=ValuePerSqFtLiving,fill=ctyname)) +
  geom_histogram(binwidth=10) + labs(x="Value per Square Foot Living")

ggplot(housing, aes(x=ValuePerSqFtLiving,fill=zip5)) +
  geom_histogram(binwidth=10) + labs(x="Value per Square Foot Living")

ggplot(housing, aes(x=ValuePerSqFtLiving,fill=ctyname)) +
  geom_histogram(binwidth=10) + labs(x="Value per Square Foot Living") +
  facet_wrap(~ctyname)

ggplot(housing, aes(x=ValuePerSqFtLiving,fill=sitetype)) +
  geom_histogram(binwidth=10) + labs(x="Value per Square Foot Living") +
  facet_wrap(~sitetype)
names(housing)

fit_1 <- lm(Sale_Price ~ sq_ft_lot ,data=housing )
fit_2 <- lm(Sale_Price ~ bedrooms*bath_full_count ,data=housing )
summary(fit_1)
summary(fit_2)

fit_1$coefficients
fit_2$coefficients

library(coefplot)
