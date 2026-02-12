<script setup>
import { ref, onMounted } from 'vue'
import axios from 'axios'
import { useToast } from 'vue-toastification'

const toast = useToast()
const API_URL = 'http://localhost:8080/api'
const API_KEY = 'SecretApiKey123'

// Configuración global de Axios para incluir el API Key
axios.defaults.headers.common['X-Api-Key'] = API_KEY

const movies = ref([])
const categories = ref([])
const loading = ref(false)

const newMovie = ref({
  titulo: '',
  descripcion: '',
  precio: 0,
  categoryId: '',
  estado: 'Activa'
})

const newCategory = ref({
  nombre: '',
  descripcion: ''
})

const editingMovie = ref(null)
const editingCategory = ref(null)

const fetchCategories = async () => {
  try {
    const response = await axios.get(`${API_URL}/categories`)
    categories.value = response.data
  } catch (error) {
    console.error('Error fetching categories:', error)
    toast.error('Error al cargar categorías')
  }
}

const fetchMovies = async () => {
  loading.value = true
  try {
    const response = await axios.get(`${API_URL}/movies`)
    movies.value = response.data
  } catch (error) {
    console.error('Error fetching movies:', error)
    toast.error('Error al cargar películas')
  } finally {
    loading.value = false
  }
}

const addCategory = async () => {
  if (!newCategory.value.nombre) {
    toast.warning('El nombre de la categoría es requerido')
    return
  }
  try {
    await axios.post(`${API_URL}/categories`, newCategory.value)
    newCategory.value = { nombre: '', descripcion: '' }
    fetchCategories()
    toast.success('Categoría agregada correctamente')
  } catch (error) {
    toast.error(error.response?.data?.detail || 'Error al agregar categoría')
  }
}

const updateCategory = async () => {
  if (!editingCategory.value.nombre) {
    toast.warning('El nombre de la categoría es requerido')
    return
  }
  try {
    await axios.put(`${API_URL}/categories/${editingCategory.value.id}`, editingCategory.value)
    editingCategory.value = null
    fetchCategories()
    fetchMovies() // Por si cambió el nombre de la categoría en el listado de películas
    toast.success('Categoría actualizada correctamente')
  } catch (error) {
    toast.error('Error al actualizar categoría')
  }
}

const deleteCategory = async (cat) => {
  const relatedMovies = movies.value.filter(m => m.categoryId === cat.id)
  let message = `¿Estás seguro de eliminar la categoría "${cat.nombre}"?`
  
  if (relatedMovies.length > 0) {
    message += `\n\n⚠️ ADVERTENCIA: Esta categoría tiene ${relatedMovies.length} películas asociadas. ¡Se ELIMINARÁN EN CASCADA todas ellas!`
  }

  if (!confirm(message)) return

  try {
    await axios.delete(`${API_URL}/categories/${cat.id}`)
    fetchCategories()
    fetchMovies()
    toast.success('Categoría y sus películas eliminadas')
  } catch (error) {
    toast.error('Error al eliminar categoría')
  }
}

const addMovie = async () => {
  if (!newMovie.value.titulo || !newMovie.value.categoryId) {
    toast.warning('Título y Categoría son requeridos')
    return
  }
  try {
    await axios.post(`${API_URL}/movies`, newMovie.value)
    newMovie.value = { titulo: '', descripcion: '', precio: 0, categoryId: '', estado: 'Activa' }
    fetchMovies()
    toast.success('Película agregada correctamente')
  } catch (error) {
    if (error.response?.data?.errors) {
      const errors = Object.values(error.response.data.errors).flat().join('. ')
      toast.error(`Validación: ${errors}`)
    } else {
      toast.error('Error al agregar película')
    }
  }
}

const updateMovie = async () => {
  if (!editingMovie.value.titulo || !editingMovie.value.categoryId) {
    toast.warning('Título y Categoría son requeridos')
    return
  }
  try {
    await axios.put(`${API_URL}/movies/${editingMovie.value.id}`, {
      id: editingMovie.value.id,
      titulo: editingMovie.value.titulo,
      descripcion: editingMovie.value.descripcion,
      precio: editingMovie.value.precioUsd || editingMovie.value.precio, // Manejar la diferencia entre read y update DTO
      categoryId: editingMovie.value.categoryId,
      estado: editingMovie.value.estado
    })
    editingMovie.value = null
    fetchMovies()
    toast.success('Película actualizada correctamente')
  } catch (error) {
    toast.error('Error al actualizar película')
  }
}

const deleteMovie = async (id) => {
  if (!confirm('¿Seguro que deseas eliminar esta película?')) return
  try {
    await axios.delete(`${API_URL}/movies/${id}`)
    fetchMovies()
    toast.success('Película eliminada')
  } catch (error) {
    toast.error('Error al eliminar película')
  }
}

const startEditCategory = (cat) => {
  editingCategory.value = { ...cat }
  window.scrollTo({ top: 0, behavior: 'smooth' })
}

const startEditMovie = (movie) => {
  editingMovie.value = { ...movie, precio: movie.precioUsd }
  window.scrollTo({ top: 0, behavior: 'smooth' })
}

onMounted(() => {
  fetchCategories()
  fetchMovies()
})
</script>

<template>
  <div class="container">
    <header>
      <img src="https://www.fragansa.com/wp-content/uploads/2025/07/Logo-Black-PPagina-Web-.png" alt="Logo" />
      <h1>Distribución de Películas</h1>
      <p class="subtitle">Gestión inteligente de catálogo y precios multimoneda</p>
    </header>

    <div class="grid">
      <!-- Sección de Categorías -->
      <section class="card">
        <h2><span>🏷️</span> {{ editingCategory ? 'Editar Categoría' : 'Nueva Categoría' }}</h2>
        
        <div class="form-group" v-if="!editingCategory">
          <input v-model="newCategory.nombre" placeholder="Nombre de Categoría" />
          <textarea v-model="newCategory.descripcion" placeholder="Descripción breve (opcional)" rows="2"></textarea>
          <button @click="addCategory" class="btn-primary">
            Agregar Categoría
          </button>
        </div>

        <div class="form-group" v-else>
          <input v-model="editingCategory.nombre" placeholder="Nombre de Categoría" />
          <textarea v-model="editingCategory.descripcion" placeholder="Descripción breve" rows="2"></textarea>
          <div class="btn-group">
            <button @click="updateCategory" class="btn-success">Guardar Cambios</button>
            <button @click="editingCategory = null" class="btn-outline">Cancelar</button>
          </div>
        </div>

        <div class="list-minimal">
          <h3>Categorías Existentes</h3>
          <ul>
            <li v-for="cat in categories" :key="cat.id" class="category-item">
              <span>{{ cat.nombre }}</span>
              <div class="item-actions">
                <button @click="startEditCategory(cat)" class="btn-icon">✏️</button>
                <button @click="deleteCategory(cat)" class="btn-icon delete">🗑️</button>
              </div>
            </li>
          </ul>
        </div>
      </section>

      <!-- Sección de Películas -->
      <section class="card">
        <h2><span>🎬</span> {{ editingMovie ? 'Editar Película' : 'Nueva Película' }}</h2>
        
        <!-- Formulario Crear -->
        <div class="form-group" v-if="!editingMovie">
          <input v-model="newMovie.titulo" placeholder="Título de la película" />
          <textarea v-model="newMovie.descripcion" placeholder="Sinopsis o detalles..." rows="2"></textarea>

          <div style="display: grid; grid-template-columns: 1fr 1fr; gap: 12px;">
            <input v-model.number="newMovie.precio" type="number" step="0.01" placeholder="Precio (USD)" />
            <select v-model="newMovie.categoryId">
              <option value="" disabled selected>Categoría</option>
              <option v-for="cat in categories" :key="cat.id" :value="cat.id">
                {{ cat.nombre }}
              </option>
            </select>
          </div>

          <select v-model="newMovie.estado">
            <option value="Activa">Estado: Activa</option>
            <option value="Inactiva">Estado: Inactiva</option>
          </select>

          <button @click="addMovie" class="btn-success">
            Guardar Película
          </button>
        </div>

        <!-- Formulario Editar -->
        <div class="form-group" v-else>
          <input v-model="editingMovie.titulo" placeholder="Título de la película" />
          <textarea v-model="editingMovie.descripcion" placeholder="Sinopsis..." rows="2"></textarea>

          <div style="display: grid; grid-template-columns: 1fr 1fr; gap: 12px;">
            <input v-model.number="editingMovie.precioUsd" type="number" step="0.01" placeholder="Precio (USD)" />
            <select v-model="editingMovie.categoryId">
              <option v-for="cat in categories" :key="cat.id" :value="cat.id">
                {{ cat.nombre }}
              </option>
            </select>
          </div>

          <select v-model="editingMovie.estado">
            <option value="Activa">Estado: Activa</option>
            <option value="Inactiva">Estado: Inactiva</option>
          </select>

          <div class="btn-group">
            <button @click="updateMovie" class="btn-success">Actualizar Película</button>
            <button @click="editingMovie = null" class="btn-outline">Cancelar</button>
          </div>
        </div>
      </section>
    </div>

    <hr />

    <!-- Listado de Películas -->
    <section class="movies-list">
      <h2>Listado de Películas</h2>
      <div v-if="loading" style="text-align: center; padding: 40px; color: var(--text-muted);">
        Cargando catálogo...
      </div>
      <div v-else class="table-container">
        <table>
          <thead>
            <tr>
              <th>Detalles de Película</th>
              <th>Categoría</th>
              <th>USD</th>
              <th>COP (Aprox)</th>
              <th>Estado</th>
              <th>Acciones</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="movie in movies" :key="movie.id">
              <td class="movie-info">
                <strong>{{ movie.titulo }}</strong>
                <p class="small">{{ movie.descripcion || 'Sin descripción' }}</p>
              </td>
              <td>{{ movie.categoryNombre }}</td>
              <td style="font-weight: 500;">${{ movie.precioUsd.toFixed(2) }}</td>
              <td class="cop-price">COP {{ movie.precioCop.toLocaleString() }}</td>
              <td>
                <span :class="['badge', movie.estado.toLowerCase()]">{{ movie.estado }}</span>
              </td>
              <td class="table-actions">
                <button @click="startEditMovie(movie)" class="btn-edit">
                  Editar
                </button>
                <button @click="deleteMovie(movie.id)" class="btn-danger">
                  Eliminar
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </section>
  </div>
</template>

<style>
@import url('https://fonts.googleapis.com/css2?family=Outfit:wght@300;400;500;600;700&display=swap');

:root {
  --primary: #0f172a;
  --primary-light: #1e293b;
  --accent: #4f46e5;
  --success: #10b981;
  --danger: #ef4444;
  --bg: #fcfcfd;
  --card-bg: #ffffff;
  --text: #1e293b;
  --text-muted: #64748b;
  --border: #e2e8f0;
  --radius: 10px;
  --shadow-sm: 0 1px 2px 0 rgb(0 0 0 / 0.05);
  --shadow: 0 4px 6px -1px rgb(0 0 0 / 0.05), 0 2px 4px -2px rgb(0 0 0 / 0.05);
  --transition: all 0.2s cubic-bezier(0.4, 0, 0.2, 1);
}

* {
  box-sizing: border-box;
}

body {
  background-color: var(--bg);
  color: var(--text);
  font-family: 'Outfit', sans-serif;
  margin: 0;
  -webkit-font-smoothing: antialiased;
}

.container {
  max-width: 1200px;
  margin: 0 auto;
  padding: 40px 20px;
}

header {
  text-align: center;
  margin-bottom: 60px;
}

header img {
  max-width: 180px;
  margin-bottom: 20px;
  filter: drop-shadow(0 2px 4px rgba(0, 0, 0, 0.05));
}

h1 {
  font-weight: 700;
  font-size: 2.2rem;
  margin: 0;
  color: var(--primary);
  letter-spacing: -0.025em;
}

.subtitle {
  color: var(--text-muted);
  font-size: 1.1rem;
  margin-top: 8px;
  font-weight: 300;
}

.grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 32px;
  margin-bottom: 60px;
}

.card {
  background: var(--card-bg);
  padding: 32px;
  border-radius: var(--radius);
  border: 1px solid var(--border);
  box-shadow: var(--shadow-sm);
  transition: var(--transition);
}

.card:hover {
  box-shadow: var(--shadow);
  border-color: #d1d5db;
}

h2 {
  font-size: 1.25rem;
  font-weight: 600;
  margin-top: 0;
  margin-bottom: 24px;
  color: var(--primary);
  display: flex;
  align-items: center;
  gap: 8px;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

input,
textarea,
select {
  padding: 12px 16px;
  border: 1px solid var(--border);
  border-radius: 8px;
  font-size: 14px;
  font-family: inherit;
  transition: var(--transition);
  background: #f8fafc;
}

input:focus,
textarea:focus,
select:focus {
  outline: none;
  border-color: var(--accent);
  background: white;
  box-shadow: 0 0 0 3px rgba(79, 70, 229, 0.1);
}

button {
  padding: 12px 24px;
  border: none;
  border-radius: 8px;
  font-weight: 600;
  font-size: 14px;
  cursor: pointer;
  transition: var(--transition);
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
}

button:active {
  transform: translateY(1px);
}

.btn-primary {
  background: var(--primary);
  color: white;
}

.btn-primary:hover {
  background: var(--primary-light);
}

.btn-success {
  background: var(--accent);
  color: white;
}

.btn-success:hover {
  filter: brightness(1.1);
}

.btn-danger {
  background: transparent;
  color: var(--danger);
  border: 1px solid transparent;
  padding: 6px 12px;
  font-size: 13px;
}

.btn-danger:hover {
  background: #fee2e2;
  border-color: #fca5a5;
}

hr {
  border: 0;
  border-top: 1px solid var(--border);
  margin: 60px 0;
}

.movies-list h2 {
  margin-bottom: 32px;
  font-size: 1.5rem;
}

.table-container {
  background: white;
  border-radius: var(--radius);
  border: 1px solid var(--border);
  overflow: hidden;
  box-shadow: var(--shadow-sm);
}

table {
  width: 100%;
  border-collapse: collapse;
}

th,
td {
  padding: 18px 24px;
  text-align: left;
}

th {
  background: #f8fafc;
  color: var(--text-muted);
  font-size: 12px;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  border-bottom: 1px solid var(--border);
}

tr:not(:last-child) td {
  border-bottom: 1px solid var(--border);
}

tr:hover td {
  background-color: #fcfcfd;
}

.movie-info strong {
  display: block;
  font-size: 15px;
  color: var(--primary);
  margin-bottom: 4px;
}

.movie-info .small {
  font-size: 13px;
  color: var(--text-muted);
}

.cop-price {
  font-weight: 700;
  color: var(--success);
  font-size: 14px;
}

.badge {
  padding: 4px 10px;
  border-radius: 6px;
  font-size: 11px;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.025em;
}

.badge.activa {
  background: #d1fae5;
  color: #065f46;
}

.badge.inactiva {
  background: #f1f5f9;
  color: #475569;
}

.list-minimal {
  margin-top: 32px;
  padding-top: 24px;
  border-top: 1px solid var(--border);
}

.list-minimal h3 {
  font-size: 14px;
  color: var(--text-muted);
  margin-bottom: 16px;
  font-weight: 600;
}

.list-minimal ul {
  list-style: none;
  padding: 0;
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.category-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  font-size: 14px;
  padding: 10px 14px;
  background: #f1f5f9;
  border-radius: 8px;
  color: var(--primary-light);
  font-weight: 500;
  transition: var(--transition);
}

.category-item:hover {
  background: #e2e8f0;
}

.item-actions {
  display: flex;
  gap: 4px;
}

.btn-icon {
  background: transparent;
  padding: 4px;
  font-size: 14px;
  border-radius: 4px;
}

.btn-icon:hover {
  background: rgba(0,0,0,0.05);
}

.btn-icon.delete:hover {
  background: #fee2e2;
}

.btn-group {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 12px;
}

.btn-outline {
  background: transparent;
  border: 1px solid var(--border);
  color: var(--text-muted);
}

.btn-outline:hover {
  background: #f8fafc;
  border-color: #cbd5e1;
}

.btn-edit {
  background: #eef2ff;
  color: var(--accent);
  border: 1px solid #e0e7ff;
  padding: 6px 12px;
  font-size: 13px;
}

.btn-edit:hover {
  background: #e0e7ff;
}

.table-actions {
  display: flex;
  gap: 8px;
}

/* Animations */
@keyframes fadeIn {
  from {
    opacity: 0;
    transform: translateY(10px);
  }

  to {
    opacity: 1;
    transform: translateY(0);
  }
}

.card,
.table-container {
  animation: fadeIn 0.5s ease-out forwards;
}

@media (max-width: 768px) {
  .grid {
    grid-template-columns: 1fr;
  }
  .btn-group {
    grid-template-columns: 1fr;
  }
}
</style>
