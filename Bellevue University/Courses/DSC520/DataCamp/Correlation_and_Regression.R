install.packages("openintro")
library(openintro)
library(ggplot2)
#Scatter plot
ggplot(data = ncbirths, aes(x = weeks,y = weight)) +
  geom_point()
#Box plot with cut() to discretize the x axis
ggplot(data = ncbirths, 
       aes(x = cut(weeks, breaks = 5),y = weight)) + 
  geom_boxplot()

# Characterizing bivariate relationships
# Form(linear,quadratic,non-linear)
# Direction(positive, or negative)
# Outliers
# scatterplots can reveal the nature of the relationship between two variables.


head(mammals)
ggplot(data = mammals, aes(x = BodyWt,y = BrainWt)) +
  geom_point()

head(mlbBat10)
ggplot(data = mlbBat10, aes(x = OBP,y = SLG)) +
  geom_point()

head(bdims)
str(bdims)
ggplot(data = bdims, aes(x = hgt,y = wgt,color=factor(sex))) +
  geom_point()

head(smoking)
str(smoking)
ggplot(data = smoking, aes(x = age,y = amtWeekdays)) +
  geom_point(na.rm = TRUE)
ggplot(data = smoking, aes(x = age,y = amtWeekdays)) +
  geom_point()

ggplot(data = mammals, aes(x = BodyWt, y = BrainWt)) +
  geom_point() + 
  coord_trans(x = "log10", y = "log10")

ggplot(data = mammals, aes(x = BodyWt, y = BrainWt)) +
  geom_point() +
  scale_x_log10() + 
  scale_y_log10()


head(mlbBat10)
ggplot(data = mlbBat10, aes(x = SB,y = HR)) +
  geom_point(alpha=0.5,position="jitter")

#install.packages("magrittr") # package installations are only needed the first time you use it
#install.packages("dplyr")    # alternative installation of the %>%
library(magrittr) # needs to be run every time you start R and want to use %>%
library(dplyr)    #
mlbBat10 %>%
  filter(SB>50 | HR >50)  %>%
  select(name,team,position,SB,HR)
# Filter for AB greater than or equal to 200
ab_gt_200 <- mlbBat10 %>%
  filter(AB >= 200) 

# Scatterplot of SLG vs. OBP
ggplot(ab_gt_200,aes(x = OBP,y = SLG)) +
  geom_point()

# Identify the outlying player
ab_gt_200 %>%
  filter(AB >= 200 & OBP < .200)

# Quantifying the strength of bivariate relationships
# Correlation
# Correlation coefficient between -1(negative) and 1(Positive correlation), .2 week amd .5 moderate
# sign -> direction
# Magnitude -> Strength

# Non-linear 
nrow(run10)
run10 %>%
  summarize(N = n(), r = cor(age, pace, use ="pairwise.complete.obs"))
run10 %>%
  filter(divPlace <= 10) %>%
  ggplot(aes(x=age,y=pace,color=gender)) +
  geom_point()


# Compute correlation
ncbirths %>%
  summarize(N = n(), r = cor(weight, mage))

# Compute correlation for all non-missing pairs
str(ncbirths)
ncbirths %>%
  summarize(N = n(), r = cor(weight, mage, use ="pairwise.complete.obs"))

# Anscombe dataset

str(anscombe)
head(anscombe)
nrow(anscombe)
ggplot(data = anscombe,aes(x=x,y=y)) +
  geom_point() +
  facet_wrap(~set)

anscombe %>%
  filter(set == 1) %>%
  ggplot(aes(x=x,y=y)) +
  geom_point()

# Compute properties of Anscombe
Anscombe %>%
  group_by(set) %>%
  summarize(
    N = n(), 
    mean_of_x = mean(x), 
    std_dev_of_x = sd(x), 
    mean_of_y = mean(y), 
    std_dev_of_y = sd(y), 
    correlation_between_x_and_y = cor(x,y))
    
# Run this and look at the plot
ggplot(data = mlbBat10, aes(x = OBP, y = SLG)) +
  geom_point()

# Correlation for all baseball players
mlbBat10 %>%
  summarize(N = n(), r = cor(OBP, SLG))    

# Run this and look at the plot
mlbBat10 %>% 
  filter(AB > 200) %>%
  ggplot(aes(x = OBP, y = SLG)) + 
  geom_point()

# Correlation for all players with at least 200 ABs
mlbBat10 %>%
  filter(AB >= 200) %>%
  summarize(N = n(), r = cor(OBP, SLG))

# Run this and look at the plot
ggplot(data = bdims, aes(x = hgt, y = wgt, color = factor(sex))) +
  geom_point() 

# Correlation of body dimensions
bdims %>%
  group_by(sex) %>%
  summarize(N = n(), r = cor(hgt, wgt))

# Run this and look at the plot
ggplot(data = mammals, aes(x = BodyWt, y = BrainWt)) +
  geom_point() + scale_x_log10() + scale_y_log10()

# Correlation among mammals, with and without log
mammals %>%
  summarize(N = n(), 
            r = cor(BodyWt, BrainWt), 
            r_log = cor(log(BodyWt), log(BrainWt)))

# Interpretation of Correlation
# if cor(x,y) < 0 as x->0 , y->infinity
# if cor(x,y) > 0 as x->infinity , y->infinity
# Spurious correlations
# Correlation does not imply causation!
# Spurious over time(variables move together over time)

# Create faceted scatterplot
#x <- rnorm(20)
#y <- rnorm(20)
#z <- seq.int(20)
x <- c()
y <- c()
z <- c()
for (i in c(1:50)) {
  x <- append(x, rnorm(20), after = length(x))
  y <- append(y, rnorm(20), after = length(y))
  z <- append(z, seq.int(20), after = length(z))
}
noise <- data.frame(x,y,z)
nrow(noise)
ggplot(data = noise,aes(x=x,y=y)) +
  geom_point() +
  facet_wrap(~z)



# Compute correlations for each dataset

noise_summary <- noise %>%
  group_by(z) %>%
  summarize(N = n(), spurious_cor = cor(x, y))
# Isolate sets with correlations above 0.2 in absolute strength
noise_summary %>%
  filter(abs(spurious_cor) > .2)
#Visualization of linear models
ggplot(data=possum,aes(y=totalL,x = tailL)) +
  geom_point() + geom_abline(intercept = 0, slope=2.5)

ggplot(data=possum,aes(y=totalL,x = tailL)) +
  geom_point() + geom_abline(intercept = 0, slope=1.5)

ggplot(data=possum,aes(y=totalL,x = tailL)) +
  geom_point() + geom_abline(intercept = 40, slope=1.3)

ggplot(data=possum,aes(y=totalL,x = tailL)) +
  geom_point() + geom_smooth(method="lm")

ggplot(data=possum,aes(y=totalL,x = tailL)) +
  geom_point() + geom_smooth(method="lm",se = FALSE)

# Scatterplot with regression line
ggplot(data = bdims, aes(x = hgt, y = wgt)) + 
  geom_point() + 
  geom_smooth(method = "lm", se = TRUE)
# Scatterplot with regression line
ggplot(data = bdims, aes(x = hgt, y = wgt)) + 
  geom_point() + 
  geom_smooth(method = "lm", se = FALSE)

# response = f(explanatory) + noise
# response = intercept + (slope*explanatory) + noise
# Y = beta0 + beta1*X + epsilon, where epsilon ~ N(0,sigma_of_epsilon)
# beta0 is intercept
# beta1 is slopw
# epsilon is noise. distribution of noise is normal which means its mean is zero 
# and a fixed standard of deviation.
# linear function that produces the fitted values is denoted as:
# Y_hat = beta0_hat + beta1_hat*X
# Y_hat is the expected value of Y and Y is the actual(observed) value.
# e = Y - Y_hat is called residuals. 
# epsilon is an unknown true quanity whereas e is known estimate of that quantity
# fitting procedure:
# given n oservations of pairs(xi,yi)...(xn,yn)
# find beta0_hat,beta1_hat that minimizes sum(square(ei)) for i=1 to n
# least squares fitting procedure
# Easy, deterministic, unique solution
# Residuals are guaranteed to sum to zero
# line must pass through (xbar,ybar)
# Key concepts:
# Y_hat is expected value given corresponding X
# Beta-hats are estimates of true, unknow betas
# rsiduals(e's) are estimates of true, unknown epsilons


N <- c(507)
r <- c(.7173011)
mean_hgt  <- c(171.1438)
sd_hgt <- c(9.407205)
mean_wgt <- c(69.14753)
sd_wgt <- c(13.34576)
# combine them together
bdims_summary <-data.frame(N,r,mean_hgt,sd_hgt,mean_wgt,sd_wgt)

bdims_summary

# Add slope and intercept
bdims_summary %>%
  mutate(slope = r*(sd_wgt/sd_hgt), 
         intercept = mean_wgt - (r*(sd_wgt/sd_hgt))*mean_hgt)


library(foreign)
Galton_DataSet = read.dta("dataverse_files/galton-stata11.dta")
str(Galton_DataSet)
Galton_DataSet$gender
head(Galton_DataSet)

Galton_men <- subset(Galton_DataSet,subset=(gender == 'M'))
Galton_women <- subset(Galton_DataSet,subset=(gender == 'F'))
# Height of children vs. height of father
ggplot(data = Galton_men, aes(x = father, y = height)) +
  geom_point() + 
  geom_abline(slope = cor(Galton_men$father,Galton_men$height)*(sd(Galton_men$height)/sd(Galton_men$father)), intercept = mean(Galton_men$height)-(cor(Galton_men$father,Galton_men$height)*(sd(Galton_men$height)/sd(Galton_men$father)) * mean(Galton_men$father))) + 
  geom_smooth(method = "lm", se = FALSE)

ggplot(data = Galton_women, aes(x = mother, y = height)) +
  geom_point() + 
  geom_abline(slope = cor(Galton_women$mother,Galton_women$height)*(sd(Galton_women$height)/sd(Galton_women$mother)), intercept = mean(Galton_women$height)-(cor(Galton_women$mother,Galton_women$height)*(sd(Galton_women$height)/sd(Galton_women$mother)) * mean(Galton_women$mother))) + 
  geom_smooth(method = "lm", se = FALSE)

# Height of children vs. height of father
ggplot(data = Galton_men, aes(x = father, y = height)) +
  geom_point() + 
  geom_abline(slope = 1, intercept = 0) + 
  geom_smooth(method = "lm", se = FALSE)

# Height of children vs. height of mother
ggplot(data = Galton_women, aes(x = mother, y = height)) +
  geom_point() + 
  geom_abline(slope = 0, intercept = 1) + 
  geom_smooth(method = "lm", se = FALSE)


# Linear model for weight as a function of height
lm(wgt ~ hgt, data = bdims)

# Linear model for SLG as a function of OBP
lm(SLG ~ OBP, data = mlbBat10)

# Log-linear model for body weight as a function of brain weight
lm(log(BodyWt) ~ log(BrainWt),data=mammals)


mod <- lm(formula = wgt ~ hgt, data = bdims)
class(mod)                                                                                            
mod
coef(mod)
summary(mod)
fitted.values(mod)
residuals(mod)
install.packages("broom")
library(broom)
augment(mod)
mod

# Mean of weights equal to mean of fitted values?
mean(bdims$wgt) == mean(fitted.values(mod))

# Mean of the residuals
mean(residuals(mod))




bdims_tidy <- augment(mod)
# Glimpse the resulting data frame
glimpse(bdims_tidy)

augment(mod) %>%
  arrange(desc(.resid)) %>%
  head()
wgt <- c(74.8)
hgt<-c(182.8)
ben <- data.frame(wgt,hgt)
# Print ben
ben

# Predict the weight of ben
predict(mod,newdata=ben)

# Add the line to the scatterplot
ggplot(data = bdims, aes(x = hgt, y = wgt)) + 
  geom_point() + 
  geom_abline(data = coefs, 
              aes(intercept = `(Intercept)`, slope = hgt),  
              color = "dodgerblue")
#sum of squares
mod_possum <- lm(totalL ~ tailL, data=possum) %>%
  augment() %>%
  summarize(SSE = sum(.resid^2),SSE_also = (n() -1) * var(.resid))

mod_possum
# Root mean squared error
# View summary of model
summary(mod)

# Compute the mean of the residuals
mean(residuals(mod))

# Compute RMSE  a.k.a. residual standard error.
sqrt(sum(residuals(mod)^2) / df.residual(mod))

#sum of squares
mod_possum <- lm(totalL ~ tailL, data=possum) %>%
  augment() %>%
  summarize(SSE = sum(.resid^2),SSE_also = (n() -1) * var(.resid))
# Null (average model) or total sum of squares
mod_null <- lm(totalL ~ 1, data = possum) %>%
  augment(possum) %>%
  summarize(SST = sum(.resid^2))
mod_null


# coefficient of determination or R-squared = 1 - SSE/SST = 1 - Var(e)/Var(y)
# SST is the null model which is total sum of squares
# SSE is

# View model summary
summary(mod)
str(bdims_tidy)
head(bdims_tidy)
# Compute R-squared
bdims_tidy %>%
  summarize(var_y =var(wgt) , var_e = var(.resid)) %>%
  mutate(R_squared = 1 - var_e/var_y)

# Unusual plots
regulars <- mlbBat10 %>%
  filter(AB > 400)
ggplot(data = regulars,aes(x = SB, y =HR)) +
  geom_point() +
  geom_smooth(method = "lm", se =0)

# leverage computation
# The leverage of an observation in a regression model is defined entirely 
# in terms of the distance of that observation from the mean of the explanatory variable. 
# That is, observations close to the mean of the explanatory variable have low leverage, 
# while observations far from the mean of the explanatory variable have high leverage. 
# Points of high leverage may or may not be influential.
mod <- lm(HR ~ SB, data = regulars)
mod %>%
  augment() %>%
  arrange(desc(.hat)) %>%
  select(HR,SB,.fitted,.resid,.hat) %>%
  head()


# influence via Cook;s distance
mod %>%
  augment() %>%
  arrange(desc(.hat)) %>%
  select(HR,SB,.fitted,.resid,.hat,.cooksd) %>%
  head()
# Rank points of high leverage
mod %>%
  augment() %>%
  arrange(desc(.hat)) %>%
  head()

# As noted previously, observations of high leverage may or may not be influential. The influence of an 
# observation depends not only on its leverage, but also on the magnitude of its residual. Recall that 
# while leverage only takes into account the explanatory variable (x), the residual depends on the response 
# variable (y) and the fitted value (y^).

# Influential points are likely to have high leverage and deviate from the general relationship between the two variables. 
# We measure influence using Cook's distance, which incorporates both the leverage and residual of each observation.
# Rank influential points
mod %>%
  augment() %>%
  arrange(desc(.cooksd)) %>%
  head()

# Create nontrivial_players
nontrivial_players <- subset(mlbBat10,subset=(AB >= 10 & OBP < 0.500))


# Fit model to new data
mod_cleaner <-lm(SLG ~ OBP , data = nontrivial_players)

# View model summary

summary(mod_cleaner)
# Visualize new model
ggplot(data = nontrivial_players,aes(x = OBP, y =SLG)) +
  geom_point() +
  geom_smooth(method = "lm")


# Not all points of high leverage are influential. While the high leverage observation corresponding to Bobby Scales 
# in the previous exercise is influential, the three observations for players with OBP and SLG values of 0 are not influential.

# This is because they happen to lie right near the regression anyway. Thus, while their extremely low OBP gives them 
# the power to exert influence over the slope of the regression line, their low SLG prevents them from using it.
# Rank high leverage points
mod %>%
  augment() %>%
  arrange(desc(.hat),desc(.cooksd)) %>%
  head()


