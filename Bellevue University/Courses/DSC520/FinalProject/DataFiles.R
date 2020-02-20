
library(ggplot2)
library(stats)
library(GGally)
library(scatterplot3d)
library("readxl")

# From chapter 19.2 R for everyone
getwd()
setwd(".\\")
getwd()
Dataset <- read_excel("DataFiles/C271 R6 norm.xlsx")
#summary(Housing)
str(Dataset)
columns <- c(names(Dataset))
columns
names(Dataset) <- gsub("\\s+","_",names(Dataset))

names(Dataset)

ggplot(Dataset, aes(x="Date",y="Rake_TC_1")) +
      geom_point()

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
