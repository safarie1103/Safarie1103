library(ggplot2)
library(stats)
library(GGally)
library(scatterplot3d)
library(lm.beta)
library("readxl")
library(foreign)
library(caTools)

getwd()
setwd(".\\")
getwd()
ThoracicSurgeryData <-read.arff("ThoraricSurgery.arff")
names(ThoracicSurgeryData)



set.seed(123)
split = sample.split(ThoracicSurgeryData$Risk1Yr, SplitRatio = 0.80)
training_set = subset(ThoracicSurgeryData, split == TRUE)
test_set = subset(ThoracicSurgeryData, split == FALSE)

glm_ThoracicSurgeryData_1 <-  glm(Risk1Yr ~ AGE, data = training_set, family = binomial())
summary(glm_ThoracicSurgeryData_1)


prob_pred = predict(glm_ThoracicSurgeryData_1, type = 'response', newdata = test_set[-17])
y_pred = ifelse(prob_pred > 0.5, 1, 0)
y_pred
# Making the Confusion Matrix
cm = table(test_set[, 17], y_pred > 0.5)

cm


glm_ThoracicSurgeryData_2 <-  glm(Risk1Yr ~ AGE + PRE30, data = training_set, family = binomial())
summary(glm_ThoracicSurgeryData_2)


prob_pred = predict(glm_ThoracicSurgeryData_2, type = 'response', newdata = test_set[-17])
y_pred = ifelse(prob_pred > 0.5, 1, 0)
y_pred
# Making the Confusion Matrix
cm = table(test_set[, 17], y_pred > 0.5)

cm