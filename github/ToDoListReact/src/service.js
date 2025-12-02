import axios from 'axios';

// ========== הגדרת כתובת ה-API כברירת מחדל ==========
// זה הפורט שרץ אצלך!
// axios.defaults.baseURL = 'http://localhost:5290';
axios.defaults.baseURL = process.env.REACT_APP_API_URL || 'http://localhost:5290';

// ========== Interceptor לטיפול בשגיאות ==========
// זה תופס כל שגיאה שחוזרת מהסרבר ומדפיס אותה
axios.interceptors.response.use(
  // אם הכל בסדר - מחזיר את התשובה
  (response) => response,

  // אם יש שגיאה - מדפיס ללוג
  (error) => {
    console.error('❌ שגיאה בקריאה לשרת:', {
      message: error.message,
      status: error.response?.status,
      data: error.response?.data
    });
    return Promise.reject(error);
  }
);

// ========== פונקציות API ==========

// קבלת כל המשימות
export const getTasks = async () => {
  const response = await axios.get('/items');
  return response.data;
};

// הוספת משימה חדשה
export const addTask = async (name) => {
  const response = await axios.post('/items', {
    name: name,
    isComplete: false
  });
  return response.data;
};

// עדכון סטטוס השלמה של משימה
export const setCompleted = async (id, isComplete) => {
  // קודם שולפים את המשימה כדי לשמור את שאר השדות
  const tasks = await getTasks();
  const task = tasks.find(t => t.id === id);

  if (!task) {
    throw new Error('משימה לא נמצאה');
  }

  // 👇 זה השינוי החשוב! המרה ל-0 או 1
  const response = await axios.put(`/items/${id}`, {
    name: task.name,
    isComplete: isComplete ? 1 : 0  // 👈 במקום isComplete לבד
  });
  return response.data;
};

// מחיקת משימה
export const deleteTask = async (id) => {
  const response = await axios.delete(`/items/${id}`);
  return response.data;
};

// ייצוא ברירת מחדל
export default {
  getTasks,
  addTask,
  setCompleted,
  deleteTask
};