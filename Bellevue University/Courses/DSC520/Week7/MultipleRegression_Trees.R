library(datasets)
library(ggplot2)
library(GGally)
library(scatterplot3d)

str(trees)
ggpairs(data=trees,columns=1:3,title="trees data")

# Minimiziong least squares
# Simple linear regression
# TreeVolume = Intercept + slope*TreeGirth + Error
fit_1 = lm(Volume ~ Girth , data=trees)
summary(fit_1)
max(trees$Volume)

ggplot(data=trees, aes(fit_1$residuals)) +
  geom_histogram(binwidth = 1, color = "black", fill = "purple4") +
  theme(panel.background = element_rect(fill = "white"),
        axis.line.x=element_line(),
        axis.line.y=element_line()) +
  ggtitle("Histogram for Model Residuals")


ggplot(data = trees, aes(x = Girth, y = Volume)) +
  geom_point() +
  stat_smooth(method = "lm", col = "dodgerblue3") +
  theme(panel.background = element_rect(fill = "white"),
        axis.line.x=element_line(),
        axis.line.y=element_line()) +
  ggtitle("Linear Model Fitted to Data")


predict(fit_1, data.frame(Girth = 18.2))

# Multiple linear regression
# TreeVolume = Intercept + slope1*TreeGirth + Slope2*TreeHeight + Error
fit_2 = lm(Volume ~ Girth + Height,data=trees)
summary(fit_2)

Girth <- seq(9,21, by=0.5) ## make a girth vector
Height <- seq(60,90, by=0.5) ## make a height vector
pred_grid <- expand.grid(Girth = Girth, Height = Height)

## make a grid using the vectors

pred_grid$Volume2 <-predict(fit_2, new = pred_grid)
pred_grid

fit_2_sp <- scatterplot3d(pred_grid$Girth, pred_grid$Height, pred_grid$Volume2, angle = 60, color = "dodgerblue", pch = 1, ylab = "Hight (ft)", xlab = "Girth (in)", zlab = "Volume (ft3)" )
fit_2_sp$points3d(trees$Girth, trees$Height, trees$Volume, pch=16)
predict(fit_2, data.frame(Girth = 18.2, Height = 72))

# Accounting for interactions
# TreeVolume = Intercept + slope1*TreeGirth + Slope2*TreeHeight + slope3*(TreeGirth*TreeHeight) + Error
# "Girth * Height” term is shorthand for “Girth + Height + Girth * Height” 
fit_3 = lm(Volume ~ Girth*Height,data=trees)
summary(fit_3)

Girth <- seq(9,21, by=0.5)
Height <- seq(60,90, by=0.5)
pred_grid <- expand.grid(Girth = Girth, Height = Height)


pred_grid$Volume3 <-predict(fit_3, new = pred_grid)
pred_grid

fit_3_sp <- scatterplot3d(pred_grid$Girth, pred_grid$Height, pred_grid$Volume3, angle = 60, color = "dodgerblue", pch = 1, ylab = "Hight (ft)", xlab = "Girth (in)", zlab = "Volume (ft3)")
fit_3_sp$points3d(trees$Girth, trees$Height, trees$Volume, pch=16)

p1 <- predict(fit_3, data.frame(Girth = 18.2, Height = 72))


# estimate the volume of a small sapling (a young tree):

p2 <- predict(fit_3, data.frame(Girth = .25, Height = 4))

p1
p2

# P2 is greater than p1 and it shouldn't be. This makes this model inaccurate.
# Accurate predictions is constrained by the range of the data we use to build our models.

