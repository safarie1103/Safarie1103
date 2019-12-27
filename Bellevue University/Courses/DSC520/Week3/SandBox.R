x <- sample(x=1:100,size=100,replace=TRUE)
x
mean(x)
y <- x
y[sample(x=1:100,size=20,replace=FALSE)] <- NA 
y
mean(y)
mean(y,na.rm=TRUE)
grades <-c(95,72,87,66)
weights <- c(1/2,1/4,1/8,1/8)
mean(grades)
weighted.mean(x=grades,w=weights)
var(x)
sum(x-mean(x)^2)/(length(x)-1)
sqrt(var(x))
sd(x)
sd(y)
sd(y,na.rm=TRUE)
min(x)
max(x)
x
median(x)
min(y)
min(y,na.rm=TRUE)
#Summary ignores NAs
summary(x)
summary(y)
library(pastecs)
stat.desc(x)
#calculate 25th and 75th quantile
quantile(x,probs=c(.25,.75))
quantile(y,probs=c(.25,.75),na.rm=TRUE)
quantile(x,probs=c(.1,.25,.5,.75,.88,.99))
library(ggplot2)
head(economics)
cor(economics['pce'],economics$psavert)
#Calculate each part of correlation
xPart <- economics$pce - mean(economics$pce)
xPart
ypart <- economics$psavert - mean(economics$psavert)
ypart
nMinusOne <- (nrow(economics)-1)
xSD <- sd(economics$pce)
ySD <- sd(economics$psavert)
#use correlation formula
sum(xPart*ypart)/(nMinusOne*xSD*ySD)
cor(economics['pce'],economics$psavert)
#Compute correlation of multiple variables
cor(economics[,c(2,4:6)])
GGally::ggpairs(economics[,c(2,4:6)])


# load the reshape package for melting the data
library(reshape2)
# load the scales package for some extra plotting features
library(scales)
# build the correlation matrix
econCor <- cor(economics[, c(2, 4:6)])
# melt it into the long format
econMelt <- melt(econCor, varnames=c("x", "y"), value.name="Correlation")
# order it according to the correlation
econMelt <- econMelt[order(econMelt$Correlation), ]
# display the melted data
econMelt


## plot it with ggplot
# initialize the plot with x and y on the x and y axes
  ggplot(econMelt, aes(x=x, y=y)) +
       # draw tiles filling the color based on Correlation
       geom_tile(aes(fill=Correlation)) +
       # make the fill (color) scale a three color gradient with muted
       # red for the low point, white for the middle and steel blue
       # for the high point
       # the guide should be a colorbar with no ticks, whose height is
       # 10 lines
       # limits indicates the scale should be filled from -1 to 1
       scale_fill_gradient2(low=muted("red"), mid="white",
        +             high="steelblue",
        +             guide=guide_colorbar(ticks=FALSE, barheight=10),
        +             limits=c(-1, 1)) +
       # use the minimal theme so there are no extras in the plot
       theme_minimal() +
       # make the x and y labels blank
       labs(x=NULL, y=NULL)
  

  