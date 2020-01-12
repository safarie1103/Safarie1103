data(tips,package="reshape2")
head(tips)  

unique(tips$sex)
unique(tips$day)
# One-Sample T-Test
# First we conduct a one-sample t-test on whether the average tip is equal to $2.50. This test essentially calculates the
# mean of data and builds a confidence interval. If the value we are testing falls within that confidence interval, then we
# can conclude that it is the true value for the mean of the data; otherwise, we conclude that it is not the true mean.

t.test(tips$tip,alternative="two.sided",mu=2.5)
nrow(tips)

## build a t distribution
randT<-rt(3000,df=NROW(tips)-1)
randT
class(randT)
tipTTest<-t.test(tips$tip,alternative="two.sided",mu=2.5)
tipTTest

# plot it
ggplot(data.frame(x=randT)) +
  geom_density(aes(x=x), fill="grey", color="grey") +
  geom_vline(xintercept=tipTTest$statistic) +
  geom_vline(xintercept=mean(randT) + c(-2, 2)*sd(randT), linetype=2)


t.test(tips$tip,alternative="greater",mu=2.5)


# Two-Sample T-Test
# More often than not the t-test is used for comparing two samples. Continuing with the tips data, we compare how female
# and male diners tip. Before running the t-test, however, we first need to check the variance of each sample. A
# traditional t-test requires both groups to have the same variance, whereas the Welch two-sample t-test can handle groups
# with differing variances.

# first just compute the variance for each group
# using the the formula interface
# calculate the variance of tip for each level of sex
male_variance<-var(tips$tip[tips$sex=="Male"])
male_variance
female_variance<-var(tips$tip[tips$sex=="Female"])
female_variance
aggregate(tip ~ sex, data=tips, var)

# now test fornormality of tip distribution
shapiro.test(tips$tip)
shapiro.test(tips$tip[tips$sex=="Male"])


# all the tests fail so inspect visually
ggplot(tips, aes(x=tip, fill=sex)) +
  geom_histogram(binwidth=.5, alpha=1/2)

ansari.test(tip ~ sex, tips)


# setting var.equal=TRUE runs a standard two sample t-test
# var.equal=FALSE (the default) would run the Welch test
t.test(tip ~ sex, data=tips, var.equal=TRUE)


library(plyr)
tipSummary <- ddply(tips, "sex", summarize,
                    tip.mean=mean(tip), tip.sd=sd(tip),
                    Lower=tip.mean - 2*tip.sd/sqrt(NROW(tip)),
                    Upper=tip.mean + 2*tip.sd/sqrt(NROW(tip)))
tipSummary


ggplot(tipSummary,aes(x=tip.mean,y=sex)) +
    geom_point() +
    geom_errorbarh(aes(xmin=Lower,xmax=Upper),height=.2)
