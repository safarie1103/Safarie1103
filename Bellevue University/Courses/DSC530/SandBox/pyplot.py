import pandas as pd
import matplotlib.pyplot as plt
import numpy as np

fig, ax = plt.subplots()
month = pd.Series(np.array(['Jan', 'feb', 'mar', 'apr', 'may', 'june', 'jul', 'aug', 'sep', 'oct', 'nov', 'dec']))
seatle_temp = pd.Series(np.array([23.3, 24.3, 26.7, 27.8, 32.12, 45.3, 56.4, 78.9, 79.4, 87.6, 99.6, 10.98]))
austin_temp = pd.Series(np.array([43.3, 34.3, 32.7, 54.8, 67.12, 45.3, 76.4, 78.9, 79.4, 17.6, 99.6, 100.98]))
month
seatle_temp
austin_temp
ax.plot(month, seatle_temp,marker="o",linestyle="--",color='b')
ax.plot(month, austin_temp,marker="v",linestyle="--",color='r')
ax.set_xlabel("Time (months)")
ax.set_ylabel("Average temperature (Fahrenheit degrees)")
ax.set_title("Weather averages by month")
plt.show()
