library(ggplot2)
library(stats)
library(GGally)
library(scatterplot3d)
library(lm.beta)
library("readxl")
library(foreign)
library(caTools)

getwd()

dataset = read.csv('binary-classifier-data.csv')
head(dataset)

#dataset$label = factor(dataset$label, levels = c(0, 1))
head(dataset)
classifier = glm(formula = label ~ .,
                 family = binomial,
                 data = dataset)
summary(classifier)
