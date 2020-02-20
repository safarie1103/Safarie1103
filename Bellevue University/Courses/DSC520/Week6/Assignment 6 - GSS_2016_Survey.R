library(ggplot2)
library(stats)
library(GGally)
library(scatterplot3d)


getwd()
setwd(".\\")
getwd()
survey <- read.csv("gss-2016.csv")



survey[1:15,c('CHILDS','SIBS','SEX')]

# Assuming 1 for males and 2 for females.
males =subset(survey,SEX==1)
females = subset(survey,SEX==2)

males[1:10,c('CHILDS','SIBS','SEX')]
females[1:10,c('CHILDS','SIBS','SEX')]
## Scatter Plot with best-fit linear regression line

ggplot(data = survey,aes(x = CHILDS, y = SIBS)) +
  geom_point(na.rm = TRUE) +
  stat_smooth(method = "lm",na.rm = TRUE, col = "dodgerblue3") +
  theme(panel.background = element_rect(fill = "white"),
        axis.line.x=element_line(),
        axis.line.y=element_line()) +
  ggtitle("Linear Model Fitted to Data")

ggplot(data = survey,aes(x = SIBS , y = CHILDS)) +
  geom_point(na.rm = TRUE) +
  stat_smooth(method = "lm",na.rm = TRUE, col = "dodgerblue3") +
  theme(panel.background = element_rect(fill = "white"),
        axis.line.x=element_line(),
        axis.line.y=element_line()) +
  ggtitle("Linear Model Fitted to Data")

mean(survey$SIBS,na.rm=TRUE)
## Scatter plot shows
### Correlatrion between CHILDS and
GGally::ggpairs(data=survey,columns=c('CHILDS','SIBS','SEX'),title="survey data")
cor(survey$CHILDS,survey$SIBS,use="complete.obs",method="pearson")
cor(survey$CHILDS,survey$SIBS,use="complete.obs",method="spearman")
cor(survey$CHILDS,survey$SIBS,use="complete.obs",method="kendall")
                                                          
#c('pearson','spearman','kendall')

survey.lm = lm(survey$CHILDS ~survey$SIBS,data=survey)
summary(survey.lm)$r.squared

fit_1 <- lm(CHILDS ~ SIBS,data=survey)
summary(fit_1)
fit_1$coefficients


round(predict(fit_1, data.frame(SIBS = 1)))

round(predict(fit_1, data.frame(SIBS = 2)))

round(predict(fit_1, data.frame(SIBS = 3)))
round(predict(fit_1, data.frame(SIBS = 5)))
round(predict(fit_1, data.frame(SIBS = 7)))
