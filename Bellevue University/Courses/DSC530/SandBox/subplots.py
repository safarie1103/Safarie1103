import pandas as pd
import matplotlib.pyplot as plt
import numpy as np

month = pd.Series(np.array(['Jan', 'feb', 'mar', 'apr', 'may', 'june', 'jul', 'aug', 'sep', 'oct', 'nov', 'dec']))
seatle_temps = [23.3, 24.3, 26.7, 27.8, 32.12, 45.3, 56.4, 78.9, 79.4, 87.6, 99.6, 10.98]
austin_temps = [43.3, 34.3, 32.7, 54.8, 67.12, 45.3, 76.4, 78.9, 79.4, 17.6, 99.6, 100.98]
seatle_temp = pd.Series(np.array(seatle_temps))
seatle25pcnt = pd.Series(np.array(seatle_temps)) * .25
seatle75pcnt = pd.Series(np.array(seatle_temps)) * .75


austin_temp = pd.Series(np.array(austin_temps))

fig,ax = plt.subplots(3,2)


ax.shape
ax[0,0].plot(month, seatle_temp,marker="o",linestyle="--",color='b')
ax[0,1].plot(month, austin_temp,marker="o",linestyle="--",color='r')

fig,ax = plt.subplots(2,1)
ax[0].plot(month, seatle25pcnt,linestyle="--",color='b')
ax[0].plot(month, seatle_temp,color='b')
ax[0].plot(month, seatle75pcnt,linestyle="--",color='b')

fig,axes = plt.subplots(2,1, sharey=True)

axes[0].plot(month, seatle_temp,marker="o",linestyle="-",color='b')
axes[0].plot(month, seatle25pcnt,marker="o",linestyle="--",color='b')
plt.show()