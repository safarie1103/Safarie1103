library(MASS)
library(ggplot2)
mammals
# Exploration
# Scatter plot of body vs brain from mammals
ggplot(mammals,aes(x=body,y=brain)) +
  geom_point()
# Scatter plot with statistics
ggplot(mammals,aes(x=body,y=brain)) +
  geom_point(alpha = 0.6) + stat_smooth(method = "lm" ,col = "red", se = FALSE) 
# Fine tuning
ggplot(mammals,aes(x=body,y=brain)) +
  geom_point(alpha = 0.6) + 
  coord_fixed() +
  scale_x_log10() +
  scale_y_log10() + 
  stat_smooth(method = "lm",
              col = "#C42126",
              se = FALSE,size = 1)
library(scales)
ggplot(mammals,aes(x=body,y=brain)) +
  annotation_logticks() +
  geom_point(alpha = 0.6) +
  coord_fixed(xlim = c(10^-3,10^4),ylim = c(10^-1,10^4)) +
  scale_x_log10(expression("Body weight (log"["10"]*"(kg))"),
                breaks = trans_breaks("log10",function(x) 10^x),
                labels = trans_format("log10",math_format(10^.x))) +
  scale_y_log10(expression("Body weight (log"["10"]*"(g))"),
                breaks = trans_breaks("log10",function(x) 10^x),
                labels = trans_format("log10",math_format(10^.x))) +
  stat_smooth(method="lm",col = "#C42126",se = FALSE, size = 1) +
  theme_classic()
                
  