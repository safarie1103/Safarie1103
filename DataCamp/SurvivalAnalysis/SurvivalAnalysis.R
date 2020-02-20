data(GBSG2, package = "TH.data")
names(GBSG2)
data(UnempDur, package = "Ecdat")
names(UnempDur)


# Check out the help page for this dataset
help(GBSG2, package = "TH.data")

help(UnempDur, package = "Ecdat")
# Look at the summary of the dataset
summary(GBSG2)
summary(UnempDur)


time <- c(5, 6, 6,2, 4, 4)
event <- c(1, 0, 0,0, 1, 1)

library("survival")
Surv(time, event)


#For all kinds of analyses:
  
library("survival")
#For pretty visualisations:
  
library("survminer")


# Count censored and uncensored data
num_cens <- table(GBSG2$cens)
num_cens

# Create barplot of censored and uncensored data
barplot(num_cens)
GBSG2$time
GBSG2$cens
# Create Surv-Object
sobj <- Surv(GBSG2$time, GBSG2$cens)

# Look at 10 first elements
sobj[1:10]
sobj
# Look at summary
summary(sobj)

# Look at structure
str(sobj)


# Load the UnempDur data
data(UnempDur, package = "Ecdat")

# Count censored and uncensored data
cens_employ_ft <- table(UnempDur$censor1)
cens_employ_ft

# Create barplot of censored and uncensored data
barplot(cens_employ_ft)

# Create Surv-Object
sobj <- Surv(UnempDur$spell, UnempDur$censor1)

# Look at 10 first elements
sobj[1:10]


# Create time and event data
time <- c(5, 6, 2, 4, 4)

event <- c(1, 0, 0, 1, 1)

Surv(time,event)

DT <- data.frame(time=time,event=event)
# Compute Kaplan-Meier estimate
km <- survfit(Surv(time, event) ~ 1,data=DT)
km
km$n.censor
km$surv
km$n.event
# Take a look at the structure
str(km)

# Create data.frame
data.frame(time = km$time, n.risk = km$n.risk, n.event = km$n.event,
           n.censor = km$n.censor, surv = km$surv)

           
library(survminer)
           
ggsurvplot(
  fit = km,
  palette = "blue", 
  linetype = 1, 
  surv.median.line = "hv", 
  risk.table = FALSE,
  cumevents = FALSE, 
  cumcensor = FALSE,
  tables.height = 0.1
  )

# Create dancedat data
dancedat <- data.frame(
  name = c("Chris", "Martin", "Conny", "Desi", "Reni", "Phil", 
           "Flo", "Andrea", "Isaac", "Dayra", "Caspar"),
  time = c(20, 2, 14, 22, 3, 7, 4, 15, 25, 17, 12),
  obs_end = c(1, 1, 0, 1, 1, 1, 1, 1, 0, 0, 0))

# Estimate the survivor function pretending that all censored observations are actual observations.
km_wrong <- survfit(Surv(time) ~ 1, data = dancedat)

# Estimate the survivor function from this dataset via kaplan-meier.
km <- survfit(Surv(time, obs_end) ~ 1, data = dancedat)

# Plot the two and compare
ggsurvplot_combine(list(correct = km, wrong = km_wrong))

# plot of the Kaplan-Meier estimate
ggsurvplot(km)

# add the risk table to plot
ggsurvplot(km, risk.table = TRUE)

# add a line showing the median survival time
ggsurvplot(km, risk.table = TRUE, surv.median.line = "hv")
# Kaplan-Meier estimate
km <- survfit(Surv(time, cens) ~ 1, data = GBSG2)

# plot of the Kaplan-Meier estimate
ggsurvplot(km)

# add the risk table to plot
ggsurvplot(km, risk.table = TRUE)

# add a line showing the median survival time
ggsurvplot(km, risk.table = TRUE, surv.median.line = "hv")



