library(ggplot2)
library(stats)
getwd()
setwd(".\\")
getwd()

survey <- read.csv("student-survey.csv")

head(survey)
# What are the elements in your data (including the categories and data types)?
# Id, Geography and POPGROUP.display.label are cetegorical variables
# ID2(int),POPGROUPID(int),RacesReported(int),HSDegree(num) and BachDegree(num) 
# Please provide the output from the following functions: str(); nrow(); ncol()
str(survey)
# Create a Histogram of the HSDegree variable using the ggplot2 package.
hist(survey$HSDegree)
# Set a bin size for the Histogram.
hist(survey$HSDegree,breaks=4)
# Include a Title and appropriate X/Y axis labels on your Histogram Plot.
hist(survey$HSDegree,main="High School Degree",xlab="HSDegree",breaks=4)
# Answer the following questions based on the Histogram produced:

# Based on what you see in this histogram, is the data distribution unimodal?
# [ANSWER] Yes because it has only one hump
# Is it approximately symmetrical?
# [ANSWER] It is non-symmetrical
# Is it approximately bell-shaped?
# [ANSWER] No
# Is it approximately normal?
# [ANSWER] No
# If not normal, is the distribution skewed? If so, in which direction?
# [ANSWER] Skewed to the left
# Include a normal curve to the Histogram that you plotted.
hist(survey$BachDegree,breaks=4)
ggplot(data=survey,aes(survey$BachDegree)) + geom_density()
# [ANSWER] This graph is bell shaped and show as normal and symmetrical
# Explain whether a normal distribution can accurately be used as a model for this data.
# [ANSWER] For bachdegree variable, we can create summary statistics such as mean and stddev 
# 
# Create a Probability Plot of the HSDegree variable.
# Set a bin size for the Histogram.
qqnorm(survey$HSDegree)
qqline(survey$HSDegree)
ggplot(data=survey,aes(survey$HSDegree)) + geom_density()
# Answer the following questions based on the Probability Plot:
# Based on what you see in this probability plot, is the distribution approximately normal? Explain how you know.
# [ANSWER] The distribution is not normal because there are points outside of the reference line
# If not normal, is the distribution skewed? If so, in which direction? Explain how you know.
# [ANSWER] The distribution is skewed to the left as shown in the density plot
# Now that you have looked at this data visually for normality, you will now quantify normality 
# with numbers using the stat.desc() function. Include a screen capture of the results produced.

summary(survey$HSDegree)
library(pastecs)
stat.desc(survey$HSDegree)
# In several sentences provide an explanation of the result produced for skew, kurtosis, and z-scores. 
# In add[ition, explain how a change in the sample size may change your explanation?
#[ANSWER] I'm sorry I  could not get stat.desc to work.  


