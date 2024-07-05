import matplotlib.pyplot as plt
import numpy as np

# Initialisation des paramètres
a = 1.4
b = 0.3
x0 = 0
y0 = 0

# Initialisation des listes pour stocker les valeurs de xn et de yn
xn_values = [x0]
yn_values = [y0]

# Calcul des 500 premières valeurs de xn et de yn
for i in range(1, 500):
    xn = yn_values[-1] + 1 - a * (xn_values[-1])**2
    yn = b * xn_values[-1]

    # Limite à huit chiffres après virgule
    xn = round(xn, 8)
    yn = round(yn, 8)
    
    xn_values.append(xn)
    yn_values.append(yn)

print("Les 500 premières valeurs de xn sont :", xn_values[:500])
print("Les 500 premières valeurs de yn sont :", yn_values[:500])

# Traçage de yn en fonction de xn
plt.figure(figsize=(10, 6))
plt.scatter(xn_values, yn_values, s=1, color='blue')
plt.xlabel('xn')
plt.ylabel('yn')
plt.title('Trajectoire de yn en fonction de xn')
plt.grid(True)
plt.show()

# Paramètres pour l'analyse de Takens
r = 5  # paramètre de délai
n = 50  # nombre de composantes du vecteur

# Fonction pour construire les vecteurs x(i)
def construire_vecteurs(xn_values, r, n):
    vecteurs = []
    for i in range(len(xn_values) - (n - 1) * r):
        vecteur = []
        for j in range(n):
            vecteur.append(xn_values[i + j * r])
        vecteurs.append(vecteur)
    return vecteurs

# Construction des vecteurs x(i)
vecteurs_liste = construire_vecteurs(xn_values, r, n)

# Convertir la liste de vecteurs en un tableau NumPy
vecteurs = np.array(vecteurs_liste)

# Calcul de la matrice de covariance Theta
Theta = np.cov(vecteurs, rowvar=False)

print("Matrice de covariance Theta calculée avec NumPy :\n", Theta)

# Calcul des valeurs propres et des vecteurs propres
valeurs_propres, vecteurs_propres = np.linalg.eig(Theta)

# Trier les valeurs propres et les vecteurs propres par ordre décroissant
indices_tries = np.argsort(valeurs_propres)[::-1]
valeurs_propres_triees = valeurs_propres[indices_tries]
vecteurs_propres_tries = vecteurs_propres[:, indices_tries]

print("Valeurs propres triées par ordre décroissant :\n", valeurs_propres_triees)
print("Vecteurs propres triés correspondants :\n", vecteurs_propres_tries)

# Calcul de l'erreur d'approximation moyenne El pour chaque valeur propre
erreurs_approximation = np.sqrt(valeurs_propres_triees + 1)

print("Erreurs d'approximation moyenne El :\n", erreurs_approximation)

# Calcul de l'indice l pour chaque valeur propre
indices_l = np.arange(len(valeurs_propres_triees)) + 1

# Tracer El en fonction de l
plt.figure(figsize=(8, 6))
plt.plot(indices_l, erreurs_approximation, marker='o')
plt.xlabel('l')
plt.ylabel('El')
plt.title("Erreur d'approximation moyenne El en fonction de l")
plt.grid(True)
plt.show()

# Trouver la première valeur de l correspondant au premier plateau
premier_plateau = np.argmax(np.diff(erreurs_approximation) <= 0)

# Afficher la première valeur de l correspondant au premier plateau
if premier_plateau > 0:
    dimension_plongement = indices_l[premier_plateau]
    print("Dimension de plongement de l'espace de phase reconstruit :", dimension_plongement)
else:
    print("Pas de plateau détecté.")

# Utilisation seulement des valeurs de xn pour l'apprentissage
X_train = np.array(xn_values[:-1])[:, np.newaxis]  # toutes les valeurs sauf la dernière pour xn
y_train = np.array(xn_values[1:])[:, np.newaxis]   # les valeurs correspondantes de xn+1

# Définition des paramètres du réseau neuronal
input_size = X_train.shape[1]  # taille de l'entrée (1 pour xn)
hidden_size = 10  # nombre de neurones dans la couche cachée
output_size = 1   # taille de la sortie (1 pour xn+1)

# Initialisation aléatoire des poids
np.random.seed(0)
W1 = np.random.randn(input_size, hidden_size)  # poids de la couche d'entrée à la couche cachée
b1 = np.zeros((1, hidden_size))                # biais de la couche cachée
W2 = np.random.randn(hidden_size, output_size) # poids de la couche cachée à la sortie
b2 = np.zeros((1, output_size))                # biais de la couche de sortie

# Définition de la fonction d'activation (ReLU)
def relu(x):
    return np.maximum(0, x)

# Définition de la fonction de perte (erreur quadratique moyenne)
def compute_loss(predictions, targets):
    return np.mean(np.square(predictions - targets))

# Définition de l'algorithme de descente de gradient
learning_rate = 0.001
epochs = 1000

losses = []

for epoch in range(epochs):
    # Forward pass
    hidden_output = relu(np.dot(X_train, W1) + b1)
    predictions = np.dot(hidden_output, W2) + b2
    
    # Calcul de la perte
    loss = compute_loss(predictions, y_train)
    losses.append(loss)
    
    # Backward pass (calcul des gradients)
    dloss_predictions = 2 * (predictions - y_train) / len(X_train)
    dW2 = np.dot(hidden_output.T, dloss_predictions)
    db2 = np.sum(dloss_predictions, axis=0, keepdims=True)
    dhidden_output = np.dot(dloss_predictions, W2.T)
    dhidden_output[hidden_output  <= 0] = 0  # gradient pour ReLU
    dW1 = np.dot(X_train.T, dhidden_output)
    db1 = np.sum(dhidden_output, axis=0, keepdims=True)
    
    # Mise à jour des poids avec la descente de gradient
    W1 -= learning_rate * dW1
    b1 -= learning_rate * db1
    W2 -= learning_rate * dW2
    b2 -= learning_rate * db2
    
    # Affichage de la perte tous les 100 epochs
    if epoch % 100 == 0:
        print(f'Epoch {epoch}, Loss: {loss}')

# Prédiction avec le modèle entraîné
hidden_output = relu(np.dot(X_train, W1) + b1)
predictions = np.dot(hidden_output, W2) + b2

# Affichage des prédictions et des valeurs réelles
print("Prédictions :", predictions.flatten())
print("Valeurs réelles de xn+1 :", y_train.flatten())

# Traçage de l'évolution de la perte
plt.figure(figsize=(8, 6))
plt.plot(losses)
plt.xlabel('Epochs')
plt.ylabel('Loss')
plt.title('Évolution de la perte au cours de l\'entraînement')
plt.grid(True)
plt.show()

# Prédictions à différents pas en avant
def predict_future_values(xn_for_prediction, nb_predictions):
    predicted_values = []
    for i in range(nb_predictions):
        xn_pred = yn_values[-1] + 1 - a * (xn_for_prediction)**2
        yn_pred = b * xn_for_prediction
        xn_pred = round(xn_pred, 8)
        yn_pred = round(yn_pred, 8)
        predicted_values.append(xn_pred)
        xn_for_prediction = xn_pred
    return predicted_values
    ##################################################################################
    # Nombre de valeurs à prédire
nb_predictions = 1
# Sélectionner les 10 dernières valeurs de xn pour prédiction
xn_for_prediction = xn_values[-nb_predictions:]

# Initialiser une liste pour stocker les valeurs prédites
predicted_values = []

# Effectuer les prédictions à un pas en avant
for i in range(nb_predictions):
    xn_pred = yn_values[-1] + 1 - a * (xn_for_prediction[i])**2
    yn_pred = b * xn_for_prediction[i]
    
    xn_pred = round(xn_pred, 8)
    yn_pred = round(yn_pred, 8)
    
    predicted_values.append(xn_pred)

# Les valeurs attendues sont les 10 valeurs suivantes de xn
expected_values = xn_values[-nb_predictions:]

# Tracer les valeurs prédites et les valeurs attendues sur le même graphe
plt.plot(range(nb_predictions), predicted_values, label='Prédictions', marker='o')
plt.plot(range(nb_predictions), expected_values, label='Attendu', marker='x')
plt.xlabel('Index')
plt.ylabel('Valeur de xn')
plt.title('Comparaison entre les valeurs prédites et attendues (à un pas en avant)')
plt.legend()
plt.grid(True)
plt.show()
    #########################################################################################

# Nombre de pas à prévoir
nb_predictions_3 = 3
nb_predictions_10 = 10
nb_predictions_20 = 20

# Prédictions à trois pas en avant
predicted_values_3 = predict_future_values(xn_values[-1], nb_predictions_3)
expected_values_3 = xn_values[-nb_predictions_3:]

# Prédictions à dix pas en avant
predicted_values_10 = predict_future_values(xn_values[-1], nb_predictions_10)
expected_values_10 = xn_values[-nb_predictions_10:]

# Prédictions à vingt pas en avant
predicted_values_20 = predict_future_values(xn_values[-1], nb_predictions_20)
expected_values_20 = xn_values[-nb_predictions_20:]

# Tracé des prédictions et des valeurs attendues
plt.figure(figsize=(10, 8))

# Tracer les valeurs prédites et les valeurs attendues sur des graphiques séparés
plt.figure(figsize=(10, 8))

# Prédictions à trois pas en avant
plt.subplot(3, 1, 1)
plt.plot(range(nb_predictions_3), predicted_values_3, label='Prédictions (3 pas en avant)', marker='o')
plt.plot(range(nb_predictions_3), expected_values_3, label='Attendu (3 pas en avant)', marker='x')
plt.xlabel('Index')
plt.ylabel('Valeur de xn')
plt.title('Prédictions à trois pas en avant')
plt.legend()
plt.grid(True)

# Prédictions à dix pas en avant
plt.subplot(3, 1, 2)
plt.plot(range(nb_predictions_10), predicted_values_10, label='Prédictions (10 pas en avant)', marker='o')
plt.plot(range(nb_predictions_10), expected_values_10, label='Attendu (10 pas en avant)', marker='x')
plt.xlabel('Index')
plt.ylabel('Valeur de xn')
plt.title('Prédictions à dix pas en avant')
plt.legend()
plt.grid(True)

# Prédictions à vingt pas en avant
plt.subplot(3, 1, 3)
plt.plot(range(nb_predictions_20), predicted_values_20, label='Prédictions (20 pas en avant)', marker='o')
plt.plot(range(nb_predictions_20), expected_values_20, label='Attendu (20 pas en avant)', marker='x')
plt.xlabel('Index')
plt.ylabel('Valeur de xn')
plt.title('Prédictions à vingt pas en avant')
plt.legend()
plt.grid(True)

plt.tight_layout()
plt.show()

