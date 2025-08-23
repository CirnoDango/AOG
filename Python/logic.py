import numpy as np
import matplotlib.pyplot as plt

def logistic(zero, x):
    x = np.array(x)
    y = np.zeros_like(x)
    pos = x > 0
    neg = x <= 0
    y[pos] = 2*(1-zero)/(1+np.exp(-2*x[pos]/(1-zero))) + 2*(zero-0.5)
    y[neg] = 2*zero/(1+np.exp(-2*x[neg]/zero))
    return y

zeros = [0.1, 0.3, 0.5, 0.7, 0.9]
x = np.linspace(-2, 2, 400)

plt.figure(figsize=(8,5))
for z in zeros:
    plt.plot(x, logistic(z, x), label=f'base={z}')
plt.title("Custom Logistic Function")
plt.xlabel("x")
plt.ylabel("y")
plt.grid(True)
plt.legend()
plt.show()
