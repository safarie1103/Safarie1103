
library(ggplot2)
library(stats)
library(GGally)
library(scatterplot3d)


housing <- read.csv('housing.csv')
head(housing)
str(housing)

housing1 <- read.csv('housing1.csv')
head(housing1)
str(housing1)

complete.cases(housing)
which(complete.cases(housing))
which(!complete.cases(housing))

complete.cases(housing1)
which(complete.cases(housing1))
which(!complete.cases(housing1))


housing <- housing1
names(housing)
ggplot(housing, aes(x=ValuePerSqFt)) +
  geom_histogram(binwidth=10) + labs(x="Value per Square Foot")


ggplot(housing, aes(x=ValuePerSqFt,fill=Boro)) +
  geom_histogram(binwidth=10) + labs(x="Value per Square Foot") +
  facet_wrap(~Boro)

library(broom)
augment(mod)
