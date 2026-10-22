import Image from 'react-bootstrap/Image';

const API_URL = import.meta.env.VITE_API_URL;

export default function HomePage() {
  return (
    <div className="text-center">
      <h1 className="display-4">Monster Munch</h1>
      <p className="lead">Munch your way to an A!</p>
      {/* Image is served from the API's wwwroot/images folder */}
      <Image
        src={`${API_URL}/images/burger-monster.png`}
        alt="Burger monster"
        fluid
        style={{ maxWidth: '300px' }}
      />
    </div>
  );
}