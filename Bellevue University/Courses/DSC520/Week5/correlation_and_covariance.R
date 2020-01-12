library(ggplot2)
head(economics)
nrow(economics)
cor(economics$pce,economics$psavert)

xPart<-economics$pce - mean(economics$pce)
yPart<-economics$psavert - mean(economics$psavert)
nMinusOne<-(nrow(economics)-1)
xSD<-sd(economics$pce)
ySD=sd(economics$psavert)
cor<-sum(xPart*yPart)/ (nMinusOne *xSD * ySD)
cor
cor(economics[,c(2,4:6)])
cor(economics[c(1:10),c(2,4:6)])
cor(economics[c(1:100),c(2,4:6)])
colnames(economics)
GGally::ggpairs(economics[,c(2,4:6)])
library(reshape2)
library(scales)
econCor<-cor(economics[,c(2,4:6)])
econCor
econMelt<-melt(econCor,varnames=c("x","y"),value.name="correlation")
econMelt
econMelt<-econMelt[order(econMelt$correlation),]
econMelt

## plot it with ggplot
# initialize the plot with x and y on the x and y axes
  
ggplot(econMelt,aes(x=x,y=y)) +
     # draw tiles filling the color based on Correlation
     geom_tile(aes(fill = correlation)) +
     # make the fill (color) scale a three color gradient with muted
     # red for the low point, white for the middle and steel blue
     # for the high point
     # the guide should be a colorbar with no ticks, whose height is
     # 10 lines
     # limits indicates the scale should be filled from -1 to 1
     scale_fill_gradient2(low=muted("red"), mid="white", 
             high="steelblue",
             guide=guide_colorbar(ticks=FALSE, barheight=10),
             limits=c(-1, 1)) +
     # use the minimal theme so there are no extras in the plot
     theme_minimal() +
     # make the x and y labels blank
     labs(x=NULL, y=NULL)
m <- c(9, 9, NA, 3, NA, 5, 8, 1, 10, 4)
n <- c(2, NA, 1, 6, 6, 4, 1, 1, 6, 7)
p <- c(8, 4, 3, 9, 10, NA, 3, NA, 9, 9)
q <- c(10, 10, 7, 8, 4, 2, 8, 5, 5, 2)
r <- c(1, 9, 7, 6, 5, 6, 2, 7, 9, 10)
# combine them together
theMat <- cbind(m, n, p, q, r)
theMat
theMat[1,]
theMat[,c(3,4,5)]
# ows with no NA;s
theMat[c(1,4,7,9,10),]
# everything rakes NA's but cor will be NA if one column value is NA
cor(theMat,use="everything")
# all.obs expects no NA's and considers value missing
cor(theMat,use="all.obs")
# complete.obs and na.or.complete removes NA's then computed cor
cor(theMat,use="complete.obs")
cor(theMat,use="na.or.complete")
# rows 1,4,7,9,10
cor(theMat[c(1,4,7,9,10),])
    
# compare "complete.obs" and computing on select rows
# should give the same result
identical(cor(theMat, use="complete.obs"),
          +           cor(theMat[c(1, 4, 7, 9, 10), ]))

# the entire correlation matrix
cor(theMat, use="pairwise.complete.obs")

# compare the entries for m vs n to this matrix
cor(theMat[, c("m", "n")], use="complete.obs")
cor(theMat[, c("m", "p")], use="complete.obs")

data(tips,package="reshape2")
head(tips)  

GGally::ggpairs(tips)

library(RXKCD)
getXKCD(which="552")

# Covariance

cov(economics$pce,economics$psavert)
cov(economics[,c(2,4:6)])
cov(economics[c(1:10),c(2,4:6)])
cov(economics[c(1:100),c(2,4:6)])
identical(cov(economics$pce,economics$psavert),
          + cor(economics$pce,economics$psavert) *
          + sd(economics$pce) * sd(economics$psavert))

          